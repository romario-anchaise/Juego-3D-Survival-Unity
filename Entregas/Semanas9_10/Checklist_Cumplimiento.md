# Verificacion de las guias - Semanas 9 y 10

Los requisitos se extrajeron de los documentos aportados por el estudiante. Este registro describe implementaciones y evidencia local; no certifica asistencia, autoria manual, evaluacion del docente ni publicacion en el foro.

| Guia | Requisito | Implementacion o evidencia |
| --- | --- | --- |
| S09 p.5 | Artifact y cinco Target sin MeshRenderer | Escena S09_Orientacion y tabla de coordenadas en Journal_S09 |
| S09 p.6 | Terrain, cerros y centro plano | Terreno principal 100 x 100 m, relieve y borde elevado |
| S09 p.6 | Pasto, tierra y sendero | Dos TerrainLayer; sendero pintado a la casa y hacia los cerros |
| S09 p.7-9 | CharacterController y script propio | PlayerController3D: gravedad manual, salto, deltaTime y movimiento relativo |
| S09 p.10 | Rig de tercera persona | CameraTarget alto y Cinemachine; evolucion del pivote anidado a seguimiento por reglas |
| S09 p.10 | Decoracion con colliders y tags | Casa y arboles con colisiones; Ground, Obstacle, Player e Interactable |
| S09 p.11 | Parametros y respuestas tecnicas | Journal_S09 |
| S09 p.11 | Rama feature/3D-World y commit | Historial Git local, ver validacion y mensaje final |
| S09 p.11 | Kata Laberinto Imposible | Escena de cubos, camara ortografica y tecla P para mostrar profundidad |
| S10 diap.4-6 | Grupo LIGHTING y tres capas | Luna principal, ambiente Trilight y Spot de acento sobre la entrada |
| S10 diap.4 | Maximo dos luces con sombras | Luna y Spot; las demas luces en tiempo real sin sombras |
| S10 diap.7 | Cinemachine, composicion, Dead Zone y damping | RotationComposer y ThirdPersonFollow en camaras de exploracion y tension |
| S10 diap.8 | Segunda camara por Trigger | Zona_Tension_Porche; prioridad 20 dentro y 5 fuera, exploracion 10 |
| S10 diap.9 | Global Volume | ACES, Bloom, Vignette y Color Adjustments; postprocesado de Main Camera |
| S10 diap.10 | Paleta emocional | Journal_S10: azul exterior y acento ambar en la entrada |
| S10 diap.11-12 | Antes/despues con mismo encuadre | Capturas renderizadas, antes reconstruido sin luces ni Volume |
| S10 diap.12 | Kata Terror, Calma y Accion | Habitación con tres perfiles y botones/teclas 1-3 |
| Teoria diap.9 | Terror, Esperanza y Misterio | La misma habitacion con perfiles 1, 4 y 5; no cambia geometria |
| Teoria diap.10 | Clasificacion de luces | Tabla razonada en Journal_S10 |
| S10 diap.12 | Rama feature/Visual-Atmosphere y commit | Historial Git local, ver validacion y mensaje final |

## Pendientes humanos

- Realizar el ejercicio de coordenadas en pareja.
- Hacer el playtest mudo de tres minutos con otra persona y completar observaciones y ajustes reales.
- Dibujar a mano la jerarquia si el docente exige ese soporte; el Journal incluye la referencia digital.
- Publicar el material en el foro desde la cuenta del estudiante.

No se han rellenado esos puntos con resultados inventados.

## Alcance de las comprobaciones

Consultar Validacion_Tecnica.txt para los resultados reales de la ejecucion automatizada. La comodidad de la camara, la comprension de la ruta y la emocion percibida se evaluan con personas, no solo con scripts.

El proyecto conserva su nombre Survival y su escena SampleScene. No se renombra el trabajo existente a Lab9_Team[N]/Level_01 porque la guia no proporciona el numero de equipo y hay referencias que preservar.
