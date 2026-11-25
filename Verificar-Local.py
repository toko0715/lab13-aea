"""Levanta los tres proyectos, ejecuta las pruebas y cierra sus procesos."""
import os
import secrets
import subprocess
import sys
import tempfile
import time
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.request import urlopen

raiz = Path(__file__).resolve().parent
os.chdir(raiz)
evidencias = raiz / "evidencias"
evidencias.mkdir(exist_ok=True)
env = dict(os.environ, Jwt__Clave=secrets.token_urlsafe(48), Jwt__Minutos="30",
           LAB15_JEFE_CORREO="jefe.lab14@example.com",
           LAB15_JEFE_CLAVE="Taller2026!Lab14",
           ConnectionStrings__TallerDB=(
               r"Server=(localdb)\MSSQLLocalDB;Database=TallerLab15DB;"
               "Integrated Security=true;TrustServerCertificate=true"))
servidor_sql = r"(localdb)\MSSQLLocalDB"
with tempfile.TemporaryDirectory() as temporal:
    for nombre in ("TallerDB.sql", "Usuarios.sql"):
        script = Path(temporal) / nombre
        sql = (raiz / nombre).read_text(encoding="utf-8")
        script.write_text(sql.replace("TallerLab13DB", "TallerLab15DB"), encoding="utf-8")
        subprocess.run(["sqlcmd", "-S", servidor_sql, "-E", "-C", "-b", "-f", "65001",
                        "-i", str(script)], check=True)

proyectos = {"Taller.Api": 5143, "Catalogo.Api": 5144, "GatewayApi": 5145}
procesos = {}

def detener(proyecto):
    proceso, log = procesos.pop(proyecto)
    if proceso.poll() is None:
        proceso.terminate()
        proceso.wait(timeout=10)
    log.close()

def esperar(url, proyecto):
    limite = time.monotonic() + 120
    while time.monotonic() < limite:
        if procesos[proyecto][0].poll() is not None:
            raise RuntimeError(f"{proyecto} terminó. Consulta evidencias/{proyecto}.log")
        try:
            with urlopen(url, timeout=1):
                return
        except HTTPError:
            return
        except (URLError, TimeoutError):
            time.sleep(0.25)
    raise RuntimeError(f"Tiempo de espera agotado para {url}")

def probar(archivo, *opciones):
    subprocess.run([sys.executable, "Probar-Gateway.py", "http://localhost:5145",
                    "http://localhost:5144", str(evidencias / archivo), *opciones],
                   env=env, check=True)

try:
    for proyecto, puerto in proyectos.items():
        dll = raiz / proyecto / "bin" / "Debug" / "net10.0" / f"{proyecto}.dll"
        if not dll.exists():
            raise RuntimeError("Ejecuta primero dotnet build Taller.slnx")
        log = (evidencias / f"{proyecto}.log").open("w", encoding="utf-8")
        try:
            proceso = subprocess.Popen(["dotnet", str(dll), "--urls", f"http://localhost:{puerto}"],
                                       cwd=raiz / proyecto, env=env, stdout=log,
                                       stderr=subprocess.STDOUT)
        except OSError:
            log.close()
            raise
        procesos[proyecto] = proceso, log
        esperar(f"http://localhost:{puerto}/", proyecto)

    probar("local.json")
    detener("Catalogo.Api")
    probar("local-caida.json", "--solo-caida")
finally:
    for proyecto in list(procesos):
        detener(proyecto)
