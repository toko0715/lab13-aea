# Matriz de permisos S14

| Endpoint | Sin token | Recepcionista | JefeTaller |
|---|---|---|---|
| POST /api/auth/registro | Sí · 201 | Sí · 201 | Sí · 201 |
| POST /api/auth/login | Sí · 200 | Sí · 200 | Sí · 200 |
| GET /api/auth/perfil | No · 401 | Sí · 200 | Sí · 200 |
| GET /api/clientes | No · 401 | Sí · 200 | Sí · 200 |
| GET /api/clientes/{id} | No · 401 | Sí · 200 | Sí · 200 |
| GET /api/clientes/{id}/vehiculos | No · 401 | Sí · 200 | Sí · 200 |
| POST /api/clientes | No · 401 | Sí · 201 | Sí · 201 |
| PUT /api/clientes/{id} | No · 401 | Sí · 204 | Sí · 204 |
| DELETE /api/clientes/{id} | No · 401 | No · 403 | Sí · 204 |
| GET /api/vehiculos | No · 401 | Sí · 200 | Sí · 200 |
| GET /api/vehiculos/{id} | No · 401 | Sí · 200 | Sí · 200 |
| POST /api/vehiculos | No · 401 | Sí · 201 | Sí · 201 |
| PUT /api/vehiculos/{id} | No · 401 | Sí · 204 | Sí · 204 |
| DELETE /api/vehiculos/{id} | No · 401 | No · 403 | Sí · 204 |

Sí supone datos válidos y recurso existente. Las reglas de negocio siguen devolviendo 409; los datos inválidos, 400; y los recursos inexistentes, 404.
