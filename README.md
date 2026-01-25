https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax

# Neongine

**Neongine** is a custom C# engine based on [Monogame](https://github.com/MonoGame/MonoGame) and the [neon](https://github.com/Azathothep/neon) ECS framework.
It features:
- Entities, Components & 
- Transform, rendering, camera, collision
- Play mode
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

Then, you only need to pass an instance of your implementation as argument to a new `NeongineApplication` and run:
```c#
using var application = new NeongineApplication(new MyGame());
application.Run();
```

You're now good to go!

### Before continuing

Like the engine, your game will need to use both [Monogame](https://github.com/MonoGame/MonoGame) and [neon](https://github.com/Azathothep/neon). I recommand reading their documentation first, to familiarize yourself with their API. 

## Game vs Editor

When building neongine, you will actually run the editor. It means that any game system you implement won't be active right away, and will required you to press `P` to enter play mode.

### Editor functionalities

In editor, you can move entities around using the little red circle that appear at their Transform's position. To disable this feature for a specific entity, add it the `NotDraggable` component (see [NotDraggable](#notdraggable) section).

Use the `WASD` keys to move the camera around, and the `I` and `O` keys to zoom in / out. Be careful however, the camera state won't reload when entering play mode.

You can also save the current scene by pressing `Ctrl+S` key. This will create a `MainScene.json` file in the `Assets` folder, which can be loaded by the `Scenes.Load(string)` static method.

### Building the final application

To build the final application without the editor, use the following `dotnet publish` parameters:
```
dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained /p:DefineConstants=NEONGINE_BUILD
```

The application created this way won't run any editor-related code.

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
Transforms store datas relative to the coordinates of the entity: **position** (Vector3), **rotation** (float) and **scale** (Vector2).

When creating an entity, you can pass coordinates as arguments to initialize its transform right away.
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

To load a sprite file into a `Texture2D`, see the [Asset](#asset-management) section.

### Velocity

An entity with the `Velocity` component will automatically move each frame by the specified Vector2 value.
```c#
entity.Add(new Velocity(Vector2.One));
```

However, the Collision System may affect the velocity's value when resolving collisions. Thus, the velocity's value might be changed at runtime by other systems. You may need to create another component and / or system to act on this component after collision resolution, for example for a bounce effect.

### AngleVelocity

Similar to `Velocity`, an entity with the `AngleVelocity` component will automatically rotate by the specified value-per-seconds each frame.
```c#
entity.Add(new AngleVelocity(90.0f)); // The entity will rotate at 90 degrees per seconds
```

Important note: any entity with an `AngleVelocity` component will be excluded from the collision system, even if they have a `Collider` component. Collision resolution for rotating entities isn't yet supported.

### Collider

You can add a `Collider` to entities for collisions and trigger detection.

Each `Collider` is defined by `Shape` object, storing the vertices coordinates of the collider.
```c#
Shape shape = new Shape([
                            new Vector2(0.25f, 1.2f),
                            new Vector2(-0.4f, 3.1f),
                            new Vector2(-0.4f, -3.1f),
                            new Vector2(0.25, -2.1f)
                        ]);
```

For basic rectangle and circle shapes, you can also pass a `Geometry` object for it to convert into the correct vertices:
```c#
Shape shape = new Shape(new Geometry(GeometryType.Rectangle, width: 1.0f, height: 2.0f));
```

You can then pass the shape to the `Collider` on creation, along with other optional parameters:
```c#
Shape shape = new Shape(...);
Collider collider1 = new Collider(shape, size: 2.0f, rotation: 45.0f); // The collider can have its own size and rotation, calculated on top of the entitie's Transform's ones
Collider collider2 = new Collider(new Geometry(GeometryType.Circle, 2.0f)); //A Geometry can be implicitely converted into a Shape
```

Finally, you can specify as last parameter if your `Collider` should be considered as a **trigger**. Trigger will detect overlapping shapes and send enter and exit events, but won't block them during collision resolution. 
```c#
Collider collider = new Collider(myShape, isTrigger: true);
```

To subscribe to collider events, you can use the following static methods:
```c#
// The EntityID given as parameter of your Action will be the other entity the specified entity is colliding / triggering with
public static void OnColliderEnter(EntityID id, Action<EntityID, Collision> action);
public static void OnColliderExit(EntityID id, Action<EntityID> action);
public static void OnTriggerEnter(EntityID id, Action<EntityID> action);
public static void OnTriggerExit(EntityID id, Action<EntityID> action);
```

### NotDraggable

Every entity is draggable in the editor's edit mode, using a small red point at its `Transform`'s position.
If you want to disable this feature for an entity, you can add it the `NotDraggable` component.

### Camera

The `Camera` class contains a static `Main` instance representing the current `Camera` used by the Rendering system.
If you need to manipulate or change it, you can simply add a `Camera` component to an entity and replace the static reference.
```c#
Camera newCamera = entity.Add<Camera>();
Camera.Main = newCamera
```

## Systems

`Components` are only designed to store datas. To update the game state, you can implement two types of system interfaces:
- `IGameUpdateSystem` have their `Update(TimeSpan)` method called each frame
- `IGameDrawSystem` have their `Draw()` method called before each render. Use it when rendering something on-screen (shapes, text, ...)

To add or remove a system to the storage, use the `Systems` static class:
```c#
public static void Add(ISystem system);
public static void Remove(ISystem system);
```

In addition, neongine gives you the possibility to create systems for editor-only execution, using the `IEditorUpdateSystem` and `IEditorDrawSystem` interfaces.

Implementing these interfaces require you to specify if you want the system to be active only in the editor's *edit mode* or also in the editor's *play mode* (for example, the EditorCollisionVisualizer shows you colliders shape both in edit and in play mode)
```c#
public bool ActiveInPlayMode { get; }
```

However, **they will never be run in the published build**.

## Serialization

[Serialize] attribute : components & systems
JsonConstructor
Save

## Scene

Currently, any created entity or registered system is present globally ; you can't store them in distinct scenes.
You can, however, request for the current scene runtime state using the following method:
```c#
RuntimeScene runtimeScene = Scenes.GetRuntime();

// Serializes all the scene datas
SceneDefinition sceneDefinition = Scenes.GetDefinition(runtimeScene);

// Unload the scene using the RuntimeScene object
Scenes.Unload(runtimeScene);

// Reload the scene using the SceneDefinition
Scenes.Load(sceneDefinition);
```


## Assets

You can use the `Assets` static class. It will load and cache your file for future asset request.
```c#
Texture2D myTexture = Assets.GetAsset<Texture2D>("path_to_sprite"); 
```

Make sure to include your sprite to the ContentManager first using [Monogame Content Buidler Pipeline](https://github.com/MonoGame/MonoGame/tree/develop/MonoGame.Framework.Content.Pipeline) that comes with Monogame, or your file won't be included when building the application.