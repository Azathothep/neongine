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
```c#
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

Transform coordinates are dependent on parent-child relationships. Changing any transform data (position, rotation or scale) will update its children's transform datas accordingly. Of course, coordinates can be accessed and modified in world space and in local-space.
```c#
Transform transform = entity.Get<Transform>();

Vector3 position = transform.WorldPosition;
float rotation = transform.WorldRotation;
Vector2 scale = transform.WorldScale;

Vector3 localPosition = transform.LocalPosition;
float localRotation = transform.LocalRotation;
Vector2 localScale = transform.LocalScale;
```

### Renderer

To attach a sprite to an entity, you can add the `Renderer` component. It takes a `Texture2D` object as argument, and optionally a scale that will be applied to the sprite (multiplied be the entity's scale).

```c#
Texture2D myTexture;
entity.Add(new Renderer(myTexture, 2.0f));
```

To load a sprite file into a `Texture2D`, you can use the `Assets` static class. It will load and cache your file for future asset request.

```c#
Texture2D myTexture = Assets.GetAsset<Texture2D>("path_to_sprite"); 
```

Make sure to include your sprite to the ContentManager first using [Monogame Content Buidler Pipeline](https://github.com/MonoGame/MonoGame/tree/develop/MonoGame.Framework.Content.Pipeline) that comes with Monogame, or your file won't be included when building the application.

### Velocity

An entity with the `Velocity` component will automatically move each frame by the specified Vector2 value.
```c#
entity.Add(new Velocity(Vector2.One));
```

However, the Collision System may affect the velocity's value when resolving collisions. Thus, the velocity's value might be changed at runtime by other systems. You may need to create another component and / or system to act on this component after collision resolution, for example for a bounce effect.

### Collision
### ...

## Game vs Editor

- Editor vs Game systems

## Scene

## Serialization

## Assets