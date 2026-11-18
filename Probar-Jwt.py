"""Pruebas de integración S14. Requiere ejecutar Usuarios.sql e iniciar la API."""
import json, ssl, sys, time, urllib.request, urllib.error
from pathlib import Path
BASE = sys.argv[1] if len(sys.argv)>1 else 'https://localhost:7143'
OUT = Path(sys.argv[2] if len(sys.argv)>2 else 'entrega/reprueba-http.json')
context = ssl._create_unverified_context()  # Solo certificado local de desarrollo
resultados = []
def probar(metodo, ruta, esperado, datos=None, token=None, caso=''):
    cabeceras = {'Content-Type':'application/json'}
    if token: cabeceras['Authorization'] = 'Bearer '+token
    req = urllib.request.Request(BASE+ruta, data=None if datos is None else json.dumps(datos).encode(), headers=cabeceras, method=metodo)
    try:
        with urllib.request.urlopen(req, context=context) as res: codigo, cuerpo = res.status, res.read().decode()
    except urllib.error.HTTPError as res: codigo, cuerpo = res.code, res.read().decode()
    resultados.append(dict(caso=caso,metodo=metodo,ruta=ruta,esperado=esperado,codigo=codigo,cuerpo=cuerpo))
    assert codigo == esperado, (caso, codigo, esperado, cuerpo)
    return json.loads(cuerpo) if cuerpo else None
try:
    tokens={}
    for rol,correo in [('Recepcionista','recepcion.lab14@example.com'),('JefeTaller','jefe.lab14@example.com')]:
        tokens[rol]=probar('POST','/api/auth/login',200,{'correo':correo,'clave':'Taller2026!Lab14'},caso='Login '+rol)['token']
        assert probar('GET','/api/auth/perfil',200,token=tokens[rol],caso='Perfil '+rol)['rol']==rol
    for ruta in ['/api/auth/perfil','/api/clientes','/api/clientes/1','/api/clientes/1/vehiculos','/api/vehiculos','/api/vehiculos/1']:
        probar('GET',ruta,401,caso='Sin token '+ruta)
    for entidad in ['clientes','vehiculos']:
        for metodo,ruta in [('POST','/api/'+entidad),('PUT','/api/'+entidad+'/1'),('DELETE','/api/'+entidad+'/1')]:
            probar(metodo,ruta,401,{} if metodo!='DELETE' else None,caso='Sin token '+metodo+ruta)
    for correo in ['recepcion.lab14@example.com','nadie@example.com']:
        probar('POST','/api/auth/login',401,{'correo':correo,'clave':'Incorrecta123!'},caso='Credenciales inválidas')
    correo='registro.'+str(time.time_ns())+'@example.com'
    probar('POST','/api/auth/registro',201,{'correo':correo,'clave':'Taller2026!Lab14'},caso='Registro público')
    probar('POST','/api/auth/registro',409,{'correo':correo.upper(),'clave':'Taller2026!Lab14'},caso='Correo único')
    probar('POST','/api/auth/registro',400,{'correo':'rol@example.com','clave':'Taller2026!Lab14','rol':'JefeTaller'},caso='Cliente no asigna rol')
    for i,(rol,token) in enumerate(tokens.items()):
        for ruta in ['/api/clientes','/api/clientes/1','/api/clientes/1/vehiculos','/api/vehiculos','/api/vehiculos/1']:
            probar('GET',ruta,200,token=token,caso=rol+' consulta')
        sufijo=str(time.time_ns())[-6:]
        cbody={'dni':str(97000000+int(sufijo)),'nombre':'Prueba JWT '+rol,'email':'prueba@example.com','telefono':'900123456'}
        c=probar('POST','/api/clientes',201,cbody,token,rol+' crea cliente'); cid=c['clienteId']
        vbody={'placa':sufijo[:3]+'-'+sufijo[3:],'marca':'Toyota','modelo':'Yaris','anio':2024,'clienteId':cid}
        v=probar('POST','/api/vehiculos',201,vbody,token,rol+' crea vehículo'); vid=v['vehiculoId']
        for entidad,ident,body in [('clientes',cid,cbody),('vehiculos',vid,vbody)]:
            probar('PUT','/api/'+entidad+'/'+str(ident),204,body,token,rol+' actualiza')
            probar('POST','/api/'+entidad,409,body,token,rol+' duplicado')
            probar('POST','/api/'+entidad,400,{},token,rol+' entrada inválida')
            probar('GET','/api/'+entidad+'/2147483647',404,token=token,caso=rol+' inexistente')
            probar('PUT','/api/'+entidad+'/2147483647',404,body,token,rol+' actualiza inexistente')
        if rol=='Recepcionista':
            probar('DELETE','/api/clientes/'+str(cid),403,token=token,caso='Recepcionista no elimina cliente')
            probar('DELETE','/api/vehiculos/'+str(vid),403,token=token,caso='Recepcionista no elimina vehículo')
        jefe=tokens['JefeTaller']
        probar('DELETE','/api/clientes/'+str(cid),409,token=jefe,caso='Cliente con vehículos')
        probar('DELETE','/api/vehiculos/'+str(vid),204,token=jefe,caso='Jefe elimina vehículo')
        probar('DELETE','/api/clientes/'+str(cid),204,token=jefe,caso='Jefe elimina cliente')
    for entidad in ['clientes','vehiculos']:
        probar('DELETE','/api/'+entidad+'/2147483647',404,token=tokens['JefeTaller'],caso='Jefe elimina inexistente')
    token=tokens['Recepcionista']
    probar('GET','/api/auth/perfil',401,token=token[:-8]+'AAAAAAAA',caso='Firma alterada')
    print(str(len(resultados))+' solicitudes correctas. Códigos: '+str(sorted(set(r['codigo'] for r in resultados))))
finally:
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(resultados,ensure_ascii=False,indent=2),encoding='utf-8')
