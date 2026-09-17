# 3D Physics Simulation

This project is a 3D **physics simulation**, built around collision detection and rigid body dynamics systems.
It implements a complete detection and physics resolution pipeline: **broadphase → narrowphase → EPA → impulses → friction and angular rotation**.

![Demo](https://media.githubusercontent.com/media/DAVID-creator-dev/Physical-simulation-Unity/refs/heads/main/docs/images/demo.gif)

---

## Main features

* **Broadphase (AABB)**: fast detection of potential collision pairs.
* **Narrowphase (GJK)**: precise computation of convex intersections.
* **EPA (Expanding Polytope Algorithm)**: retrieval of the normal and penetration depth.
* **Impulse-based resolution**:

  * Contact point computation (with barycentric interpolation).
  * Application of linear and angular impulses.
  * Bounce handling (restitution).
* **Dynamic and static friction**
* **Angular rotation** with inertia tensor.

---

## Game / simulation controls

| Key               | Action                                    |
| ----------------- | ----------------------------------------- |
| **Z / Q / S / D**  | Move the character                        |
| **Space**          | Fire a ball                               |
| **R**              | Select an object                          |
