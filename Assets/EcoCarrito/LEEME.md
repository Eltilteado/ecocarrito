# EcoCarrito — juego 2D de tres carriles

Preparado para Unity 6.3 LTS (6000.3.6f1), la versión de tu captura.

## Edición visual y música

Nueva portada ilustrada propia, integrada a la intro y al menú. Botones coloreados con contornos claros, sombra y tipografía gruesa; paneles de puntos, combo, tiempo y final coordinados con la misma paleta.

Los residuos pasan de 0,65 a 1,10 unidades de altura (+69%). Cada residuo lleva un marco por TIPO: plástico azul, papel amarillo, vidrio verde y peligroso rojo. El marco NO cambia para coincidir con el carril: sirve para detectar residuos incorrectos. El recolector crece de 1,25 a 1,65 unidades.

La música Beyond_the_Final_Limit.mp3 adjuntada por el usuario está incluida, suena en bucle desde la intro y tiene botón MÚSICA ON/OFF. Su volumen es independiente de los efectos y baja al pausar. Ajusta musicVolume, wasteHeight y collectorHeight en EcoCatalog. Portada: Art/UI/cover_arcade.png; fuente: Art/Fonts/ArcadeBold.ttf.

## Corrección de materiales y arranque

El paquete anterior tenía 31 referencias externas al catálogo y al material con type: 3, cuando esos assets nativos necesitan type: 2. Se corrigieron en la escena y prefabs. Además, el arranque recupera el catálogo por Resources y conecta jugador, generador, interfaz, cámara y efectos antes de la partida.

Si actualizas tu proyecto existente: sal de Play, importa TODOS los archivos y espera a que compile. Vuelve a abrir la escena desde Assets/EcoCarrito/Scenes/EcoCarrito.unity, usa EcoCarrito > 0. Reparar materiales y conexiones y Ctrl+S. La reparación no cambia la disposición de tus objetos. No borres tu proyecto.

## Opción A: importar el paquete en tu proyecto

1. En Unity: Assets > Import Package > Custom Package.
2. Selecciona EcoCarrito_3Carriles.unitypackage e importa TODOS los archivos.
3. Espera a que termine de compilar.
4. Abre Assets/EcoCarrito/Scenes/EcoCarrito.unity con doble clic.
5. Pulsa EcoCarrito > 0. Reparar materiales y conexiones y guarda con Ctrl+S.
6. Expande EcoCarrito > MundoEcoCarrito en Hierarchy: los objetos ya están guardados.
7. Abre la pestaña Game, elige 16:9 y pulsa Play.
8. Tras la intro, pulsa ENTER o el botón JUGAR. Haz clic en Game si el teclado no responde.

CORRECCIÓN: la escena incluye todos los objetos guardados, visibles y editables ANTES de Play. No hace falta generar el mundo. Selecciona Fondo o Recolector en Hierarchy y pulsa F para enfocar en Scene. Las diez muestras de residuos sirven como vista previa y se ocultan al iniciar la partida. La opción Armar escena completa RECONSTRUYE la escena: úsala solo si deseas volver a la disposición inicial; guarda una copia antes si ya la modificaste.

## Opción B: abrir el proyecto completo

1. Descomprime EcoCarrito_Proyecto_Unity.zip.
2. En Unity Hub: Projects > Add > Add project from disk.
3. Selecciona la carpeta EcoCarrito, que contiene Assets, Packages y ProjectSettings.
4. Ábrela con Unity 6.3 LTS (6000.3.6f1).
5. Espera la importación y abre Assets/EcoCarrito/Scenes/EcoCarrito.unity.
6. Pulsa Play y ENTER.

## Animaciones e intro

Al pulsar Play se presenta una intro de 6,5 segundos: título animado, tres tarjetas de reciclaje, el contenedor con seis frames y residuos en movimiento. Enter o SALTAR la omite. Después aparece el menú; JUGAR inicia una cuenta atrás de 3 segundos que no consume el tiempo de partida.

Durante la partida: el contenedor abre la tapa al recoger, rebota y se inclina al cambiar de carril; los residuos oscilan y giran, los aerosoles parpadean, los aciertos producen partículas y los errores agitan brevemente la cámara. Las capas de ciudad tienen un movimiento suave.

Ajustes: introSeconds/countdownSeconds en EcoCatalog, framesPerSecond/bobAmount en Recolector y parallaxAmount/shakeStrength/burstParticles en Animaciones_y_efectos. Las animaciones están implementadas en C# con los sprites originales; no requieren Animator.

## Mecánica definitiva

- Tres carriles horizontales: arriba azul/plástico, centro amarillo/papel-cartón y abajo verde/vidrio.
- El recolector se mantiene a la izquierda. Cada pulsación ↑ o ↓ cambia al carril vecino; W y S también funcionan.
- Los residuos vienen de derecha a izquierda. Aparecen mezclados en TODOS los carriles.
- Recoger un residuo del tipo de ese carril suma puntos. Recoger uno incompatible resta 12 de limpieza y rompe el combo.
- Los aerosoles se esquivan en los tres carriles; tocar uno resta 20.
- Dejar pasar objetos no resta limpieza. Esquivar es parte del juego.
- El combo aumenta el multiplicador cada cuatro aciertos consecutivos, hasta x4.
- Tiempo: 60 s. Limpieza inicial: 100. Termina por tiempo o limpieza agotada.
- P o Esc: pausa. Enter: iniciar, continuar o reintentar según el estado.
- Menú, pausa, final, reinicio, sonidos y récord local incluidos.
- No se usan 1/2/3 ni entregas manuales.

## Qué viene armado

- Escena EcoCarrito.unity con cámara, fondos, carriles, recolector, sistemas y muestras editables, ya guardados.
- Prefabs Recolector y Residuo.
- Catálogo con 10 residuos, 6 frames del contenedor, 6 capas de ciudad y 3 sonidos.
- 14 scripts C#, separados por función, incluyendo una herramienta de editor.
- Texturas configuradas como Sprite, Filter Mode Point y sin compresión.
- Material de sprites. El código y la herramienta eligen el shader según Built-in o URP.
- Cámara 2D con ajuste 16:9. Interfaz de juego y menú generados por EcoHud; no requiere TextMeshPro ni descargar sus recursos.
- Solo se usan módulos integrados de Unity; el paquete se adapta al Input System o al Input Manager de tu proyecto.

## Archivos y objetos

| Archivo | Función |
|---|---|
| EcoCatalog.cs | Tipos, sprites, sonidos y ajustes de partida |
| EcoAssets.cs | Carga por rutas y recuperación de recursos |
| EcoMusic.cs | Música en bucle, volumen independiente y silencio persistente |
| Editor/EcoRepair.cs | Reasigna recursos usando el importador y serializador de Unity |
| EcoInput.cs | Teclas; compatibilidad con los dos sistemas de entrada |
| EcoPlayer.cs | Cambio de carril, indicador de color y animación |
| EcoWaste.cs | Movimiento y recogida; detecta cruces aunque baje el framerate |
| EcoSpawner.cs | Apariciones, tipos mezclados y velocidad progresiva |
| EcoGame.cs | Puntos, combo, limpieza, tiempo, pausa y final |
| EcoHud.cs | Intro animada, cuenta atrás, marcador y menús |
| EcoEffects.cs | Parallax, partículas de acierto/error y vibración de cámara |
| EcoCamera.cs | Ajuste a pantallas con proporción diferente |
| EcoBootstrap.cs | Construye el mundo y conecta componentes |
| Editor/EcoSceneBuilder.cs | Arma y guarda la escena/prefabs; verifica recursos |

Jerarquía generada: EcoCarrito > MundoEcoCarrito > Main Camera, Fondo, Carriles, Recolector, Residuos_activos, Sistemas y Marcador_y_menus.

Los paneles del marcador son IMGUI y no aparecen como hijos Canvas en la jerarquía. Los objetos de juego y el fondo son GameObjects con sprites editables.

## Mejorar el juego

- Fondo y colores: edita MundoEcoCarrito/Fondo y Carriles en Scene, fuera de Play.
- Recolector: cambia SpriteRenderer/Sprite en Contenedor_con_ruedas y también collectorFrames en EcoCatalog si deseas reemplazar la animación.
- Residuos: reemplaza los PNG o asigna otros sprites en wastes de EcoCatalog. Las muestras del editor no controlan la partida.
- Prefab Residuo: personaliza el hijo Sprite para nuevas animaciones o efectos.
- Textos y paneles: se editan en EcoHud.cs; no son objetos Canvas.
- Haz cambios fuera de Play para que Unity los guarde.
- Los PNG originales y paquetes fuente vienen en Materiales_EcoCarrito.zip. Conserva los créditos de Clint Bellanger si distribuyes el juego.

## Ajustar la dificultad

Selecciona Assets/EcoCarrito/Resources/EcoCarrito/EcoCatalog.asset en el Inspector.
Puedes cambiar duración, puntos, daño, frecuencia, velocidad, tiempo de aceleración y proporción de objetos compatibles.
El generador limita cada oleada a uno o dos carriles y separa oleadas al menos 0,35 s. Una oleada no ocupa los tres carriles, pero la dificultad todavía debe ajustarse jugando.

## Comprobaciones al abrir

1. EcoCarrito > 3. Verificar recursos debe mostrar un mensaje correcto en Console.
2. Pulsar ↑ y ↓ cambia solo al carril vecino y respeta los extremos.
3. Recoger plástico en azul suma puntos; papel en azul resta limpieza.
4. Esc detiene el reloj y los residuos. Continuar los reanuda.
5. Al reintentar se reinician tiempo, limpieza, puntos, combo y residuos.
6. En Scene, antes de Play, deben verse el fondo, tres carriles, el recolector y diez muestras de residuos. Las muestras desaparecen al jugar.

## Si algo no se ve

- Importa todos los archivos del paquete, incluidos Resources, .asset, materiales y prefabs.
- El proyecto completo utiliza Built-in. Si importas en URP y aparecen sprites rosas en Scene, selecciona SpriteMaterial.mat y elige un shader de sprites Unlit de URP. Al jugar, el arranque adapta el material automáticamente.
- El proyecto completo utiliza Built-in 2D. Puedes importar el unitypackage en un proyecto 2D URP.
- El control de teclado necesita que Game tenga el foco. El menú también admite el mouse.
- Si tu proyecto desactivó ambos sistemas de entrada, habilita Input Manager o Input System en Active Input Handling.
- No renombres Resources/EcoCarrito hasta adaptar las rutas de carga.

## Créditos

Residuos: Recycle items art by Clint Bellanger, CC BY 3.0.
https://opengameart.org/content/recycle-items-set
https://creativecommons.org/licenses/by/3.0/
Modificaciones: sprites separados de la hoja original, renombrados y escalados para esta escena.

Contenedores: KARSIORI STUDIO — FREE Pixel Art Trash Pack - Animated, CC0.
https://karsiori.itch.io/free-pixel-art-trash-pack-animated
Fondo: KARSIORI STUDIO — City Parallax Background, CC0.
https://karsiori.itch.io/free-pixel-art-city-background-pack
Portada cover_arcade.png: creada mediante la herramienta de generación de imágenes para este proyecto.
Fuente ArcadeBold.ttf: DejaVu Sans Bold; licencia adjunta en LICENCIA_FUENTE.txt.
Música Beyond_the_Final_Limit.mp3: archivo proporcionado por el usuario; separado de los sonidos Kenney.

Sonidos: Kenney — Interface Sounds, CC0.
https://kenney.nl/assets/interface-sounds

Los colores se usan como reglas visuales del juego; no representan una norma universal de contenedores. El tema se relaciona con el ODS 12.

## Estado de verificación

Se revisaron la sintaxis C#, las referencias GUID, los tipos de referencia de assets nativos/importados, la integridad del unitypackage y los recursos del proyecto. No se pudo ejecutar el editor Unity en el entorno de creación: la compilación y la prueba real de Play deben confirmarse al abrirlo en tu Unity. No se incluye un ejecutable compilado para Windows.
