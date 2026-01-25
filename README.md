https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax

# Neongine

**Neongine** is a custom C# engine based on [Monogame](https://github.com/MonoGame/MonoGame) and the [neon](https://github.com/Azathothep/neon) ECS framework.
It features:
- Entities, Components & Systems
- Serialization
- Asset loading / unloading
- Scenes
- ...

You can find an example pong game made using the engine [here](https://github.com/Azathothep/neongine-pong)!

## Getting Started

Create a new Monogame project, clone the neongine repo and add the `neongine` reference to your project.

Create a new class implementing the `IGame` interface. The will be the entrypoint for your game.
```c#
public class MyGame: IGame {
    public int WindowWidth;
    public int WindowHeight;

    public void EditorLoad();
    public void GameLoad();
}
```

Then, you only need to pass an instance of it as argument to a new `NeongineApplication` and run:
```
using var application = new NeongineApplication(new MyGame());
application.Run();
```

You're now good to go!

### Before continuing

Like the engine, your game will need to use both [Monogame](https://github.com/MonoGame/MonoGame) and [neon](https://github.com/Azathothep/neon). I recommand reading their documentation first, to familiarize yourself with their API. 

## Entities

Entities in your game will be represented by an EntityID object. They can hold components, have a parent-child relationship and be deactivated if required (see [neon](https://github.com/Azathothep/neon) documentation for more informations).

To create a new neongine entity, use
```c#
EntityID entity = Neongine.Entity();
```

The returned neongine entity will be initialized with two components: a `Name` and a `Transform`.

## Components

Components are objects attached to one and only one entity. They can be easily queried in batch using neon's [QueryBuilder](https://github.com/Azathothep/neon#queries).

Neongine provides you with the following set of base components:

### Transform

Each neongine entity is initialized with a `Transform`.
Transforms store datas relative to the coordinates (Vector3), rotation (float) and scale (Vector2) of the entity.

When creating an entity, you can pass transform datas as arguments to initialize it right away.
```c#
EntityID entity = Neongine.Entity(name: "my_entity", position: Vector3.One, rotation: 45.0f, scale: Vector2.One)
```

Transform coordinates are dependent on parent-child relationships. When a parent's transform data change (position, rotation or scale), it changes its children's transform datas accordingly. Of course, coordinates can be accessed and modified in world space or in local-space.
```c#
Transform transform = entity.Get<Transform>();

Vector3 position = transform.WorldPosition;
float rotation = transform.WorldRotation;
Vector2 scale = transform.WorldScale;

Vector3 localPosition = transform.LocalPosition;
float localRotation = transform.LocalRotation;
Vector2 localScale = transform.LocalScale;
```

## Builtin components and systems

### Transform
### Renderer
### Velocity
### Collision
### ...

## Game vs Editor

- Editor vs Game systems

## Scene

## Serialization

## Assets