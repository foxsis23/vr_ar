# Лабораторна робота 1 — Unity

## Як запустити

1. Відкрити проєкт в Unity 6000.5.9f1 (пакет Cinemachine підтягнеться автоматично).
2. Меню **Lab1 → Build Scene** — генерується сцена `Assets/Scenes/Lab1.unity`.
3. Натиснути Play.

Керування: `WASD` / стрілки — рух, `Space` — стрибок, `Left Shift` — прискорення.

## Відповідність завданням

| Завдання | Реалізація |
|---|---|
| Проста сцена з 3D-об'єктами та світлом | `Lab1SceneBuilder.CreateGround/CreateObstacles`, Directional Light + Point Light |
| Скрипти для анімації об'єкта | `Rotator.cs` (обертання + коливання), `PatrolMover.cs` (рух між точками) |
| Керування з клавіатури | `PlayerController.cs`, новий Input System (`Keyboard.current`) |
| CinemachineCamera | `Main Camera` + `CinemachineBrain`, об'єкт `CinemachineCamera` з `CinemachineFollow` та `CinemachineRotationComposer`, Follow/LookAt = Player |
| Повноцінне переміщення (вправо-вліво, вверх-вниз) | Rigidbody, рух по осях X/Z + стрибок по Y |
| Зіткнення з об'єктами | `OnCollisionEnter` (перешкоди, батут), `OnTriggerEnter` (монети, фініш), колайдери + стіни по периметру |
| Теги та різні дії | `Coin` — +1 очко і знищення; `Obstacle` — −10 здоров'я; `Bouncer` — підкидання вгору; `Finish` — вивід результату |
| Власний Prefab | `Assets/Prefabs/Coin.prefab` (тег `Coin`, тригер, скрипт `Rotator`), 6 екземплярів на сцені |

## Файли

- `Assets/Scripts/PlayerController.cs` — керування, фізика, реакція на теги
- `Assets/Scripts/Rotator.cs` — анімація обертання/коливання
- `Assets/Scripts/PatrolMover.cs` — анімація патрулювання
- `Assets/Scripts/GameManager.cs` — рахунок, здоров'я, HUD
- `Assets/Editor/Lab1SceneBuilder.cs` — генератор сцени, матеріалів, тегів і префабу
