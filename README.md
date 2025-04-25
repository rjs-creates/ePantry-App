# Smart Inventory Scale System

A two-component system that combines a **Raspberry Pi-powered physical scale** with a **Xamarin.Android mobile app** for efficient tracking and management of consumable items through barcode scanning and real-time weight integration.

## 📱 Xamarin.Android App (C#)

The mobile app is built using Xamarin.Android with C#, offering a cross-platform compatible solution with a clean and interactive UI. Features include:

- **Authentication**: Login and signup screens for user access.
- **Inventory List**: Displays a dynamic list of available and consumed items.
- **Barcode Scanning**: Scan item barcodes to identify and log them.
- **Scale Integration**: Communicates with the Raspberry Pi to receive item weights.
- **User Profile**: Manage account settings and personal data.

## 🍓 Raspberry Pi Scale

The Raspberry Pi acts as the hardware backend for the scale component. It performs:

- **Real-Time Weight Measurement**: Utilizes a load cell and HX711 module.
- **Data Communication**: Sends weight data to the Android app via a network socket.

## 🔧 Tech Stack

- **Xamarin.Android** (C#)
- **Raspberry Pi** (Python)
- **HX711 + Load Cell** (Weight sensor)
- **Socket Communication** (Between Pi and app)
- **Mysql** (For local persistence)

## 🎥 Demo
https://github.com/user-attachments/assets/f2948e99-ef1c-4b75-bff1-acd5e14759aa


