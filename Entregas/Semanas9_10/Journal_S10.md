# Journal: lenguaje de la luz - Semana 10

## Paleta emocional del proyecto

- Emoción dominante: terror.
- Key Light: luna azul pálida. Comunica frío y soledad, conservando visibilidad del terreno.
- Fill: ambiente Trilight azul grisáceo, con sombras legibles.
- Accent: foco ámbar sobre la entrada. Contrasta con el exterior azul y propone la casa como refugio y destino.
- Sombras: suaves y con fuerza moderada. Conservan volumen sin convertir los caminos en negro absoluto.
- Color Adjustments: saturación -18 y contraste 5 en la escena principal.
- Tonemapping: ACES. Bloom sutil y viñeta reducida para mantener legible la periferia.

La puerta más cálida destaca sobre el bosque frío. La hipótesis es que este contraste guía hacia la casa. Esa hipótesis todavía debe comprobarse con un tester humano; no se atribuyen decisiones de navegación a personas que no han participado.

## Presupuesto de luces

Todas las luces están bajo `--- LIGHTING ---`. Solo la luna y el foco de la entrada proyectan sombras. Las lámparas de la casa y el reflejo tenue que acompaña al personaje son Realtime sin sombras. El reflejo sigue la cámara mediante un componente y mantiene su posición dentro del grupo de luces en la jerarquía.

Las lámparas estáticas podrían pasarse a Baked como optimización adicional. En esta entrega se limita el número de fuentes Realtime con sombras; no se afirma haber realizado un bake ni APV.

## Cinemachine

- Exploración: CinemachineCamera, ThirdPersonFollow, seguimiento sobre el hombro y FOV 55.
- Composición: RotationComposer, personaje desplazado hacia un tercio, Dead Zone de 0.15 × 0.12 y damping 0.5.
- Zona de tensión: Trigger delante del porche. Eleva la prioridad de la segunda cámara de 5 a 20. Al salir vuelve a 5, por debajo de la cámara de exploración (10).
- Segunda cámara: FOV 40 y encuadre más cerrado, con protección contra obstáculos.
- CinemachineBrain mezcla las cámaras con EaseInOut durante 0.8 segundos.

No se anima la cámara fotograma a fotograma: se definen reglas y prioridades. Hay que recorrer el porche y las esquinas en el playtest para evaluar comodidad; un script no puede certificar que ninguna persona sienta mareo.

## Antes y después

Las capturas `S10_Nivel_Antes_Sin_Luces.png` y `S10_Nivel_Despues.png` usan la misma cámara temporal, posición, orientación y FOV. El estado «antes» se reconstruye desactivando las luces y el Volume; no es una captura histórica del proyecto. Los cambios se revierten antes de guardar la escena final.

## Por qué usar Tonemapping

La escena puede producir intensidades HDR que exceden el rango visible de la pantalla. ACES comprime ese rango y controla cómo se representan las luces y los colores brillantes. Está activado como pide la práctica. El motor no requiere Tonemapping para ejecutar cualquier proyecto URP; aquí es un requisito visual y de la entrega.

## Dev Kata S10: Habitación Emocional

Abre `Assets/Practicas_S09_S10/Escenas/S10_Habitacion_Emocional.unity`, pulsa Play y selecciona una emoción con los botones o las teclas 1 a 5.

| Tecla | Emoción | Decisión visual |
| --- | --- | --- |
| 1 | Terror | Luz fría desde abajo, desaturación y contraste |
| 2 | Calma | Luz cálida, menor contraste y Bloom suave |
| 3 | Acción | Mayor saturación, contraste y acento complementario |
| 4 | Esperanza | Luz cálida alta y exposición más clara |
| 5 | Misterio | Una fuente lateral, más información en sombra |

Los tres primeros perfiles cubren el Kata. Terror, Esperanza y Misterio cubren la actividad de teoría. Todos usan la misma geometría. El cambio selecciona un VolumeProfile y modifica exclusivamente la iluminación y el ambiente.

## Ejercicio de clasificación de luces

| Fuente de la guía | Decisión |
| --- | --- |
| Sol exterior | Realtime con sombras |
| Antorcha fija | Baked |
| Lámpara que se apaga en un combate | Realtime sin sombras |
| Neón decorativo fijo | Baked |
| Linterna del jugador | Realtime con sombras |

## Reflexión y validación pendiente

El exterior original ocultaba demasiado el suelo. Se aumentó la luz ambiental y lunar y se redujo la viñeta para hacer legibles el camino y los árboles. Después se añadió un acento sobre la entrada. Estos son cambios observados en renders del proyecto, no resultados de una encuesta.

Pendiente: registrar dónde se pierde un compañero, si interpreta la puerta como destino y si la cámara le resulta cómoda. Completar Playtest_Pendiente.md y ajustar el foco o el damping a partir de lo que ocurra realmente.
