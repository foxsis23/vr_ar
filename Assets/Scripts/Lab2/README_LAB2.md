# Лабораторна робота 2 — VR-сцена: пересування та маніпуляція об'єктами

Стек: Unity 6000.5.9f1, **Unity XR Interaction Toolkit 3.6.1** + OpenXR 1.18.0.
Замість Meta XR Simulator використано штатний **XR Interaction Simulator** з семплів XRI
(працює в редакторі на macOS без шолома), а синтетична кімната генерується скриптом.

## Як запустити

1. Відкрити проєкт в Unity 6000.5.9f1 — пакети XRI/OpenXR підтягнуться з `Packages/manifest.json`.
2. Меню **Lab2 → 1. Setup XR Project** — імпорт семплів `Starter Assets` і `XR Interaction Simulator`,
   резервування Interaction Layer 31 = `Teleport`.
3. Меню **Lab2 → 2. Build VR Scene** — генерується сцена `Assets/Scenes/Lab2_VR.unity`.
4. Натиснути Play (фокус має бути на вікні Game). Керування XR Interaction Simulator:

   | Дія | Клавіші |
   |---|---|
   | Рух голови (HMD) | `W` `A` `S` `D`, вгору/вниз — `E` / `Q` |
   | Огляд | права кнопка миші (утримувати або перемкнути) + рух миші; стрілки — поворот з клавіатури |
   | Вибір пристрою для керування | `H` — голова, `[` — ліва рука, `]` — права рука, `Tab` — цикл по пристроях |
   | Дії лівої руки з клавіш правої | утримувати `Shift` |
   | Grip (взяти предмет) | `G` |
   | Trigger | `T` |
   | Стік контролера | `I` / `K` — вперед/назад, `J` / `L` — вліво/вправо |
   | Кнопки контролера | `1` `2` — Primary/Secondary, `M` — Menu, `3` `4` — натискання стіків |
   | Швидка дія | `` ` `` — вибір дії, `Space` — виконати |
   | Довідка по керуванню | `Y` — меню Input Selection, `X` — меню дій |

5. Для реального шолома: **Lab2 → 4. Open XR Plug-in Management** → увімкнути **OpenXR**
   для потрібної платформи (для Quest — Android + Meta Quest feature group).

## Відповідність завданням

| Завдання | Реалізація |
|---|---|
| Нова VR-сцена в наявному проєкті | `Assets/Scenes/Lab2_VR.unity`, генератор `Lab2VrSceneBuilder` |
| Імпорт SDK (Meta XR All-in-One **або Unity XRI**) | XRI 3.6.1 + OpenXR 1.18.0 у `Packages/manifest.json`, семпли Starter Assets |
| Симулятор простору (Meta XR Simulator / Synthetic Room) | `XR Interaction Simulator` (семпл XRI) + синтетична кімната 8×8×3 м: підлога, стіни, стеля, стіл, полиця, два світильники |
| Віртуальна камера, відстеження голови (XR Rig / OVRCameraRig) | префаб `XR Origin (XR Rig)`: XR Origin + Camera Offset + Main Camera з Tracked Pose Driver, контролери з інтеракторами |
| Переміщення телепортацією | `TeleportationArea` на всій підлозі + три `TeleportationAnchor` (стіл, кут, полиця) з `MatchOrientation.TargetUpAndForward`, Interaction Layer 31 `Teleport` |
| Плавний рух із затіненням периферії (Comfort Vignette) | `ContinuousMoveProvider` + `SnapTurnProvider` рига, префаб `TunnelingVignette` під камерою; провайдери руху, поворотів і телепортації додані в `TunnelingVignetteController.locomotionVignetteProviders` |
| Брати 3D-об'єкти руками, утримувати, кидати | 4 предмети з `Rigidbody` + `XRGrabInteractable` (`MovementType.VelocityTracking`, `throwOnDetach`, `throwVelocityScale = 1.5`), скрипт `GrabReporter` |
| Зона точного розміщення (Socket Interactor / Snap Zone) | `XRSocketInteractor` на столі з trigger-колайдером, snapping-радіус 0.15 м, підсвічування станів — `SnapZone` |

## Файли

- `Assets/Editor/Lab2/Lab2Setup.cs` — імпорт семплів XRI, Interaction Layer для телепортації
- `Assets/Editor/Lab2/Lab2VrSceneBuilder.cs` — збірка сцени: риг, локомоція, телепорт-зони, симулятор
- `Assets/Editor/Lab2/Lab2RoomBuilder.cs` — синтетична кімната, меблі, предмети, snap zone, HUD
- `Assets/Scripts/Lab2/VrHud.cs` — панель на стіні: підказки, лічильники, журнал подій
- `Assets/Scripts/Lab2/GrabReporter.cs` — події взяття/утримання/кидка предмета
- `Assets/Scripts/Lab2/SnapZone.cs` — індикація зони фіксації (вільна / наведення / зайнята)
- `Assets/Scripts/Lab2/TeleportReporter.cs` — журналювання телепортацій
