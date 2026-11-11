"""Pruebas HTTP del laboratorio. Uso: python Probar-Api.py URL CARPETA_SALIDA."""
import concurrent.futures
import json
import sys
import urllib.error
import urllib.request
import uuid
from pathlib import Path

base = sys.argv[1] if len(sys.argv) > 1 else "https://localhost:7143"
salida = Path(sys.argv[2] if len(sys.argv) > 2 else "entrega")
salida.mkdir(parents=True, exist_ok=True)
resultados = []
comprobaciones = []
cliente_id = None
vehiculo_id = None
clientes_concurrentes = []


def solicitar(metodo, ruta, datos=None):
    cuerpo = None if datos is None else json.dumps(datos).encode()
    peticion = urllib.request.Request(
        base + ruta, data=cuerpo, method=metodo,
        headers={"Content-Type": "application/json"} if cuerpo else {},
    )
    try:
        respuesta = urllib.request.urlopen(peticion, timeout=20)
    except urllib.error.HTTPError as error:
        respuesta = error
    with respuesta:
        texto = respuesta.read().decode()
        return respuesta.status, dict(respuesta.headers), json.loads(texto) if texto else None


def caso(nombre, metodo, ruta, esperado, datos=None):
    estado, cabeceras, respuesta = solicitar(metodo, ruta, datos)
    resultados.append({
        "nombre": nombre, "metodo": metodo, "ruta": ruta,
        "esperado": esperado, "estado": estado, "entrada": datos,
        "respuesta": respuesta, "location": cabeceras.get("Location"),
    })
    assert estado == esperado, (nombre, estado, respuesta)
    return respuesta, cabeceras


def verificar(condicion, nombre):
    assert condicion, nombre
    comprobaciones.append(nombre)


cliente = {"dni": "73130001", "nombre": "Cliente de prueba", "email": "prueba@example.com", "telefono": "912345678"}
vehiculo = {"placa": "LAB-913", "marca": "Toyota", "modelo": "Yaris", "anio": 2024, "clienteId": 1}
try:
    listado, _ = caso("Clientes · listado", "GET", "/api/clientes", 200)
    verificar(any(c["dni"] == "40000001" for c in listado), "DTO Cliente incluye DNI y datos de ejemplo")
    caso("Clientes · detalle", "GET", "/api/clientes/1", 200)
    filtrados, _ = caso("Clientes · filtro por nombre", "GET", "/api/clientes?nombre=Ana", 200)
    verificar(len(filtrados) == 1 and filtrados[0]["nombre"] == "Ana Torres", "Filtro de clientes")
    caso("Cliente inexistente", "GET", "/api/clientes/999999", 404)
    creado, cabeceras = caso("Crear cliente", "POST", "/api/clientes", 201, cliente)
    cliente_id = creado["clienteId"]
    verificar(cabeceras["Location"].endswith("/api/Clientes/" + str(cliente_id)), "Location apunta al cliente creado")
    caso("Cliente creado · ruta Location", "GET", "/api/clientes/" + str(cliente_id), 200)
    caso("DNI duplicado", "POST", "/api/clientes", 409, cliente)
    caso("Cliente inválido", "POST", "/api/clientes", 400,
         {"dni": "123", "nombre": "", "email": "correo-invalido", "telefono": "1"})
    cliente_editado = cliente | {"nombre": "Cliente actualizado"}
    caso("Actualizar cliente", "PUT", "/api/clientes/" + str(cliente_id), 204, cliente_editado)
    actualizado, _ = caso("Cliente actualizado · persistencia", "GET", "/api/clientes/" + str(cliente_id), 200)
    verificar(actualizado["nombre"] == "Cliente actualizado" and actualizado["clienteId"] == cliente_id, "Map de actualización conserva ID")
    caso("Actualizar cliente inexistente", "PUT", "/api/clientes/999999", 404, cliente)
    caso("DNI duplicado al actualizar", "PUT", "/api/clientes/" + str(cliente_id), 409, cliente | {"dni": "40000001"})
    caso("Actualizar cliente inválido", "PUT", "/api/clientes/" + str(cliente_id), 400, cliente | {"email": "invalido"})
    caso("Cliente con vehículos", "DELETE", "/api/clientes/1", 409)
    caso("Eliminar cliente inexistente", "DELETE", "/api/clientes/999999", 404)
    anidados, _ = caso("Ruta anidada", "GET", "/api/clientes/1/vehiculos", 200)
    verificar(len(anidados) == 2 and all(v["clienteId"] == 1 for v in anidados), "Ruta anidada filtra por propietario")
    caso("Ruta anidada sin cliente", "GET", "/api/clientes/999999/vehiculos", 404)
    vacios, _ = caso("Cliente sin vehículos", "GET", f"/api/clientes/{cliente_id}/vehiculos", 200)
    verificar(vacios == [], "Colección vacía de cliente existente")

    listado, _ = caso("Vehículos · listado", "GET", "/api/vehiculos", 200)
    verificar(all(set(v) == {"vehiculoId", "placa", "marca", "modelo", "anio", "clienteId", "clienteNombre"} for v in listado),
              "DTO Vehiculo expone ClienteNombre y evita navegación JSON")
    caso("Vehículo · detalle", "GET", "/api/vehiculos/1", 200)
    filtros, _ = caso("Vehículos · filtros combinados", "GET", "/api/vehiculos?placa=ABC&marca=Toyota&clienteId=1", 200)
    verificar(len(filtros) == 1 and filtros[0]["clienteNombre"] == "Ana Torres", "Filtros y MapFrom de propietario")
    caso("Vehículo inexistente", "GET", "/api/vehiculos/999999", 404)
    caso("Cliente no registrado", "POST", "/api/vehiculos", 409, vehiculo | {"clienteId": 999999})
    caso("Placa duplicada", "POST", "/api/vehiculos", 409, vehiculo | {"placa": "ABC-123"})
    caso("Vehículo inválido", "POST", "/api/vehiculos", 400, vehiculo | {"placa": "?", "anio": 1900, "clienteId": 0})
    creado, cabeceras = caso("Crear vehículo", "POST", "/api/vehiculos", 201, vehiculo | {"clienteId": cliente_id, "placa": "lab-913"})
    vehiculo_id = creado["vehiculoId"]
    verificar(cabeceras["Location"].endswith("/api/Vehiculos/" + str(vehiculo_id)), "Location apunta al vehículo creado")
    verificar(creado["placa"] == "LAB-913" and creado["clienteNombre"] == "Cliente actualizado", "POST normaliza placa y devuelve nombre relacionado")
    caso("Placa duplicada con minúsculas", "POST", "/api/vehiculos", 409, vehiculo | {"placa": "lab-913"})
    caso("Actualizar vehículo", "PUT", "/api/vehiculos/" + str(vehiculo_id), 204, vehiculo | {"modelo": "Yaris actualizado", "clienteId": 2})
    actualizado, _ = caso("Vehículo actualizado · nuevo propietario", "GET", "/api/vehiculos/" + str(vehiculo_id), 200)
    verificar(actualizado["clienteNombre"] == "Bruno Rojas" and actualizado["modelo"] == "Yaris actualizado",
              "PUT actualiza entidad existente y MapFrom refleja nuevo propietario")
    caso("Actualizar vehículo inexistente", "PUT", "/api/vehiculos/999999", 404, vehiculo)
    caso("Placa duplicada al actualizar", "PUT", "/api/vehiculos/" + str(vehiculo_id), 409, vehiculo | {"placa": "ABC-123"})
    caso("Actualizar vehículo con cliente inexistente", "PUT", "/api/vehiculos/" + str(vehiculo_id), 409, vehiculo | {"clienteId": 999999})
    caso("Actualizar vehículo inválido", "PUT", "/api/vehiculos/" + str(vehiculo_id), 400, vehiculo | {"anio": 1900})
    caso("Eliminar vehículo", "DELETE", "/api/vehiculos/" + str(vehiculo_id), 204)
    caso("Vehículo eliminado", "GET", "/api/vehiculos/" + str(vehiculo_id), 404)
    vehiculo_id = None
    caso("Eliminar vehículo inexistente", "DELETE", "/api/vehiculos/999999", 404)
    caso("Eliminar cliente", "DELETE", "/api/clientes/" + str(cliente_id), 204)
    caso("Cliente eliminado", "GET", "/api/clientes/" + str(cliente_id), 404)
    cliente_id = None

    dni_concurrente = str(80000000 + int(uuid.uuid4().hex[:6], 16) % 9999999)
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as ejecutor:
        respuestas = list(ejecutor.map(
            lambda _: solicitar("POST", "/api/clientes", cliente | {"dni": dni_concurrente}), range(2)))
    clientes_concurrentes = [cuerpo["clienteId"] for estado, _, cuerpo in respuestas if estado == 201]
    verificar(sorted(estado for estado, _, _ in respuestas) == [201, 409], "Solicitudes concurrentes con mismo DNI crean un registro y devuelven 409 al duplicado")

    openapi, _ = caso("Contrato OpenAPI", "GET", "/swagger/v1/swagger.json", 200)
    schemas = openapi["components"]["schemas"]
    verificar("Cliente" not in schemas and "Vehiculo" not in schemas, "Swagger muestra DTOs y omite entidades")
    endpoints = [(metodo, ruta) for ruta, operaciones in openapi["paths"].items()
                 for metodo in operaciones if metodo in ("get", "post", "put", "delete")]
    verificar(len(endpoints) == 11, "Once endpoints REST documentados")
    verificar({r["estado"] for r in resultados} >= {200, 201, 204, 400, 404, 409}, "Se probaron los seis códigos requeridos")
finally:
    if vehiculo_id is not None:
        solicitar("DELETE", f"/api/vehiculos/{vehiculo_id}")
    if cliente_id is not None:
        solicitar("DELETE", f"/api/clientes/{cliente_id}")
    for identificador in clientes_concurrentes:
        solicitar("DELETE", f"/api/clientes/{identificador}")
    (salida / "resultados.json").write_text(
        json.dumps({"casos": resultados, "comprobaciones": comprobaciones}, ensure_ascii=False, indent=2), encoding="utf-8")
print(f"{len(resultados)} solicitudes verificadas y {len(comprobaciones)} comprobaciones adicionales.")
