module GeoVisualise.Sample

open System.IO

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open ReadElevationData
open Input
open FreeCamera

type Content =
    {
        SpriteBatch: SpriteBatch
        SpriteFont: SpriteFont
        Effect: Effect
        AspectRatio: single

        Vertices: VertexPosition []
        Indices: int []
        MinX: single
        MaxX: single
        MinZ: single
        MaxZ: single

        World: Matrix
        Projection: Matrix
    }

let loadContent (_this: Game) device (graphics: GraphicsDeviceManager) =
    let ascStream = new StreamReader(@"data\TL11\TL11.asc")
    let data = readAsc ascStream
    let vertices, minX, maxX, minZ, maxZ = ConvertToVertices.convert data
    let indices = ConvertToVertices.indices data.NumCols data.NumRows

    {
        Effect = _this.Content.Load<Effect>("Effects/effects")
        SpriteFont = _this.Content.Load<SpriteFont>("Fonts/Arial")
        SpriteBatch = new SpriteBatch(device)
        AspectRatio = device.Viewport.AspectRatio

        Vertices = vertices
        Indices = indices

        MinX = minX
        MaxX = maxX
        MinZ = minZ
        MaxZ = maxZ

        World = Matrix.Identity
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 10000.0f)
    }

type State =
    {
        ShowParameters: bool
        Exiting: bool
        Camera: FreeCamera
    }

let initialState content =
    let startingPosition = Vector3(0.5f * (content.MinX + content.MaxX), 500.0f, 0.5f * (content.MinZ + content.MaxZ))
    { ShowParameters = false; Exiting = false; Camera = FreeCamera(startingPosition, 0.0f, 0.0f) }

let update (input: Input) gameContent (gameTime: GameTime) gameState =
    let time = float32 gameTime.TotalGameTime.TotalSeconds

    { gameState with
        ShowParameters = if input.JustPressed(Keys.P) then not gameState.ShowParameters else gameState.ShowParameters
        Exiting = input.Quit
        Camera = gameState.Camera.Updated(input, time)
    }

let showParameters gameContent =
    let colour = Color.DarkSlateGray

    gameContent.SpriteBatch.Begin()
    gameContent.SpriteBatch.DrawString(gameContent.SpriteFont, "GeoVisualise", new Vector2(0.0f, 0.0f), colour)
    gameContent.SpriteBatch.End()

let draw (device: GraphicsDevice) gameContent gameState (gameTime: GameTime) =
    let time = (single gameTime.TotalGameTime.TotalMilliseconds) / 100.0f

    do device.Clear(Color.LightGray)

    gameContent.Effect.CurrentTechnique <- gameContent.Effect.Techniques.["Coloured"]

    gameContent.Effect.Parameters.["xWorld"].SetValue(gameContent.World)
    gameContent.Effect.Parameters.["xView"].SetValue(gameState.Camera.ViewMatrix)
    gameContent.Effect.Parameters.["xProjection"].SetValue(gameContent.Projection)

    gameContent.Effect.CurrentTechnique.Passes |> Seq.iter
        (fun pass ->
            pass.Apply()
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, gameContent.Vertices, 0, gameContent.Vertices.Length, gameContent.Indices, 0, gameContent.Indices.Length / 3)
        )

    if gameState.ShowParameters then showParameters gameContent
