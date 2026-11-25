# 🏝️ Resort Empire

A resort management game with an old-school indie look and feel. Build your resort, keep your guests happy, and grow it into an empire.

> **Status:** In development

## About

Resort Empire is a 2D resort management game built in Unity. You take over a small resort and expand it into a thriving destination by placing buildings, managing facilities, and serving the guests who arrive.

The game uses a retro indie visual style, with guests and staff navigating the resort using Unity's navigation system (NavMeshPlus for 2D).

## Features

<!-- Edit this list to match what is actually implemented -->
- 🏗️ Build and expand your resort
- 🧍 Guests and staff that move around the resort with AI pathfinding
- 💰 Resort management and economy
- 🎨 Retro indie game art style

## Built With

- [Unity](https://unity.com/) (2D)
- C#
- [NavMeshPlus](https://github.com/h8man/NavMeshPlus) — 2D navigation / pathfinding
- Unity AI Navigation package

## Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download)
- Unity Editor version: `<check ProjectSettings/ProjectVersion.txt>`
- Git (and Git LFS if the repo uses it, see `.gitattributes`)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/HumairAli26/Resort-Empire.git
   ```
2. Open **Unity Hub**, click **Add → Add project from disk**, and select the cloned `Resort-Empire` folder.
3. Open the project with the matching Unity Editor version. Unity will regenerate the `Library` folder on first launch, which can take a few minutes.
4. Open the main scene from `Assets/Scenes/` and press **Play**.

### Building

1. Go to **File → Build Settings**.
2. Choose your target platform and add the scenes to the build.
3. Click **Build** (or **Build and Run**).

## Project Structure

```
Resort-Empire/
├── Assets/        # Scenes, scripts, sprites, prefabs, audio
├── Library/       # Unity-generated cache
├── Logs/          # Unity logs
└── .vscode/       # VS Code editor settings
```

## How to Play

<!-- Fill in your controls and game loop -->
| Action | Control |
| ------ | ------- |
| Move camera | _TBD_ |
| Place building | _TBD_ |
| Select / interact | _TBD_ |

## Roadmap

- [ ] Core resort building
- [ ] Guest needs and satisfaction
- [ ] Staff management
- [ ] Economy balancing
- [ ] Save / load
- [ ] Sound and music
- [ ] More buildings and resort upgrades

## Contributing

Contributions, issues, and feature requests are welcome.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m "Add some AmazingFeature"`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the [MIT License](LICENSE).

## Author

**Humair Ali** — [@HumairAli26](https://github.com/HumairAli26)