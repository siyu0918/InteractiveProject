# InteractiveProject

**Gesture-Controlled Boid Simulation with Unity and MediaPipe**  

This repository showcases an interactive project that combines machine learning-based gesture recognition with a real-time boid (flocking) simulation in Unity. The project leverages MediaPipe for hand tracking and gesture interpretation, communicating these insights to Unity to dynamically control a flock of birds.

**Project Overview**  

The core of this project lies in creating a seamless interaction between human hand gestures and a virtual bird flock. Users can manipulate the behavior of the boids using both static and dynamic hand gestures, offering an intuitive and engaging experience.

**Key Features:**  

Machine Learning-Powered Gesture Recognition:   
Utilizes a trained machine learning model to accurately identify various static and dynamic hand gestures.

MediaPipe Integration:   
Employs MediaPipe for robust and real-time hand tracking, providing precise landmark data.

Unity Communication:   
Establishes a smooth communication pipeline between the gesture recognition system (likely Python) and Unity, enabling real-time control of in-game elements.

Dynamic Boid Simulation:   
Implements a realistic boid algorithm (flocking behavior) for the bird movements.

Interactive Attractor:   
The bird flock's movement is directly influenced by your finger's position, acting as an attractor that the birds follow.

Gesture-Triggered Effects:   
Different dynamic gestures trigger distinct boid behaviors:

Rotational Flock Behavior:   
Specific dynamic gestures (e.g., clockwise or counter-clockwise movements) cause the flock to gather and rotate in the corresponding direction.

Click-Triggered Burst:   
A "click" gesture (e.g., a quick tap) makes the bird flock suddenly gather and then disperse, creating a striking visual effect.

**How It Works**  

Hand Tracking & Gesture Recognition:   
MediaPipe tracks the user's hand landmarks in real-time. These landmarks are fed into a pre-trained machine learning model, which classifies the current hand pose as either a static or dynamic gesture.

Data Transmission:   
The recognized gesture and finger position data are transmitted to the Unity application (e.g., via UDP).

Unity Simulation:   
In Unity, the received data is used to update the boid simulation:

The finger's position dictates the attractor point for the birds.

Recognized dynamic gestures trigger specific boid behaviors, such as rotational movement or the gather-and-disperse effect.
