"""Genera la configuración HTTPS del gateway a partir de los tres dominios."""
import argparse
import json
from pathlib import Path
from urllib.parse import urlsplit

parser = argparse.ArgumentParser()
parser.add_argument("gateway")
parser.add_argument("taller")
parser.add_argument("catalogo")
args = parser.parse_args()

def origen(valor):
    url = urlsplit(valor)
    if (url.scheme != "https" or not url.hostname or url.username or url.password
            or url.port not in (None, 443) or url.path not in ("", "/")
            or url.query or url.fragment):
        parser.error("Usa orígenes HTTPS públicos sin ruta, por ejemplo https://app.onrender.com")
    return url.hostname

gateway, taller, catalogo = map(origen, (args.gateway, args.taller, args.catalogo))
carpeta = Path(__file__).resolve().parent / "GatewayApi"
config = json.loads((carpeta / "ocelot.json").read_text(encoding="utf-8"))
for ruta in config["Routes"]:
    destino = ruta["DownstreamHostAndPorts"][0]
    destino["Host"] = taller if destino["Port"] == 5143 else catalogo
    destino["Port"] = 443
    ruta["DownstreamScheme"] = "https"
config["GlobalConfiguration"]["BaseUrl"] = f"https://{gateway}"
salida = carpeta / "ocelot.Cloud.json"
salida.write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")
print(salida)
