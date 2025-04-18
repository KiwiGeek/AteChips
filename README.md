# AteChips 🕹️🔊

AteChips is a high-performance, modern CHIP-8 emulator with a focus on **retro aesthetics**, hardware agnosticism and **real-time configurability** via ImGui.

> Built for hackers, retro nerds, and audio tinkerers. ⚡

---

## 🚀 Features

- 🎮 **Full CHIP-8 instruction set** with timing control
- 🖥️ **ImGui-based debugging and configuration UI**
- 🌈 **CRT-style shaders** for authentic visual effects
- 🔊 **Custom audio engine** powered by:
  - Dynamic waveform synthesis
  - Adjustable pitch, volume, and morphing
  - Pulse width modulation (PWM)
  - Real-time waveform preview

---

## 🧠 Architecture

### 🎨 Visual Layer
- `Display` class handles rendering, ImGui overlay, and shaders- Waveform previews drawn with consistent timing
- Support for High-DPI, dynamic window scaling, and visualized memory

### 🔊 Audio System
- Component-driven `ISoundDevice` architecture
- Real-time signal synthesis using `DynamicSoundEffectInstance`
- Each waveform implements a phase-based `GetWaveformSample()`
- Volume normalized using RMS (in progress)
- Ring modulation, detuning, and noise variants supported

### 🧩 Emulator Core
- Modular `IUpdatable` / `IDrawable` systems
- Frequency-driven component dispatch
- Memory-mapped hardware plan (WIP)

---

## 📦 Serialization (WIP)

All sound settings are persistable to JSON, enabling:
- 🎼 Saving and loading user presets
- 📁 Import/export of waveform patches
- 🔁 Rehydration of UI state across sessions

---

## 💬 Get Involved

This is a solo/experimental project at the moment, but if you’re excited about:

- Retro emulation
- Emulator UX
- ImGui-based tools

…then jump in, fork it, or reach out!

---

## 📄 License

Unlicense License — free for use, modification, and hacking. Attribution appreciated.

---

*Made with 🧡 for chiptunes, nostalgia, and waveforms that go pew.*
