"""Ejecuta pruebas HTTP sin guardar claves ni tokens en las evidencias."""
import argparse
import json
import os
import uuid
from pathlib import Path
from urllib.error import HTTPError
from urllib.request import Request, urlopen

parser = argparse.ArgumentParser()
parser.add_argument("gateway", help="Origen del gateway")
parser.add_argument("catalogo", help="Origen del catálogo para verificar el acceso directo")
parser.add_argument("salida", type=Path)
parser.add_argument("--solo-caida", action="store_true")
args = parser.parse_args()
base = args.gateway.rstrip("/") + "/gateway"
resultados = []

def verificar(nombre, metodo, url, esperado, token=None, datos=None):
    headers = {}
    if token:
        headers["Authorization"] = "Bearer " + token
    if datos is not None:
        headers["Content-Type"] = "application/json"
    solicitud = Request(url, data=None if datos is None else json.dumps(datos).encode(),
                        headers=headers, method=metodo)
    try:
        respuesta = urlopen(solicitud, timeout=90)
    except HTTPError as error:
        respuesta = error
    with respuesta:
        estado = respuesta.code
        ubicacion = respuesta.headers.get("Location")
        cuerpo = respuesta.read()
    resultados.append(dict(prueba=nombre, metodo=metodo, url=url,
                           esperado=esperado, recibido=estado))
    print(f"{estado} {metodo:6} {url} [{nombre}]", flush=True)
    if estado != esperado:
        raise AssertionError(f"{nombre}: se esperaba {esperado}, recibido {estado}")
    return json.loads(cuerpo) if cuerpo and estado < 400 else None, ubicacion

def iniciar_sesion(datos):
    cuerpo, _ = verificar("Login", "POST", base + "/auth/login", 200, datos=datos)
    return cuerpo["token"]

def pruebas():
    clave = uuid.uuid4().hex
    if args.solo_caida:
        token = iniciar_sesion(dict(correo=os.environ["LAB15_JEFE_CORREO"],
                                    clave=os.environ["LAB15_JEFE_CLAVE"]))
        verificar("Catálogo detenido", "GET", base + "/servicios", 502, token)
        verificar("Taller sigue disponible", "GET", base + "/vehiculos", 200)
        return

    jefe = iniciar_sesion(dict(correo=os.environ["LAB15_JEFE_CORREO"],
                               clave=os.environ["LAB15_JEFE_CLAVE"]))
    cuenta = dict(correo=f"lab15-{uuid.uuid4().hex}@example.com", clave=clave)
    verificar("Registro público", "POST", base + "/auth/registro", 201, datos=cuenta)
    recepcion = iniciar_sesion(cuenta)
    verificar("Clientes públicos", "GET", base + "/clientes", 200)
    verificar("Vehículos públicos", "GET", base + "/vehiculos", 200)
    verificar("Catálogo protegido", "GET", base + "/servicios", 401)
    verificar("Catálogo directo sin token", "GET",
              args.catalogo.rstrip("/") + "/api/servicios", 401)
    verificar("Catálogo con token", "GET", base + "/servicios", 200, recepcion)
    verificar("Detalle de catálogo", "GET", base + "/servicios/1", 200, recepcion)
    verificar("Servicio inexistente", "GET", base + "/servicios/999999", 404, recepcion)
    verificar("Perfil", "GET", base + "/auth/perfil", 200, recepcion)
    cliente = dict(dni=str(10000000 + uuid.uuid4().int % 90000000),
                   nombre="Cliente de prueba", email=cuenta["correo"], telefono="900000001")
    verificar("Escritura sin token", "POST", base + "/clientes", 401, datos=cliente)
    creado, ubicacion = verificar("Crear cliente", "POST", base + "/clientes",
                                   201, recepcion, cliente)
    ruta = f"/gateway/clientes/{creado['clienteId']}"
    url = args.gateway.rstrip("/") + ruta
    try:
        if not ubicacion or ubicacion.lower() != ruta:
            raise AssertionError(f"Location debe apuntar al gateway: {ubicacion}")
        verificar("Leer cliente creado desde Location", "GET",
                  args.gateway.rstrip("/") + ubicacion, 200)
        verificar("Actualizar cliente", "PUT", url, 204, recepcion,
                  dict(cliente, nombre="Cliente actualizado"))
        verificar("Recepción no puede eliminar", "DELETE", url, 403, recepcion)
    finally:
        verificar("Jefe elimina el cliente", "DELETE", url, 204, jefe)
    verificar("Cliente eliminado", "GET", url, 404)

try:
    pruebas()
finally:
    args.salida.parent.mkdir(parents=True, exist_ok=True)
    args.salida.write_text(json.dumps(resultados, indent=2, ensure_ascii=False) + "\n",
                           encoding="utf-8")
