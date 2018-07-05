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

        Vertices: VertexPositionNormalTexture []
        Indices: int []
        MinX: single
        MaxX: single
        MinY: single
        MaxY: single

        World: Matrix
        Projection: Matrix

        LightDirection: Vector3
    }

let loadContent (_this: Game) device (graphics: GraphicsDeviceManager) =
    let ascStream = new StreamReader(@"data\TL11\TL11.asc")
    let data = readAsc ascStream
    let vertices, minX, maxX, minY, maxY = ConvertToVertices.convert data
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
        MinY = minY
        MaxY = maxY

        World = Matrix.Identity
        Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 10000.0f)

        LightDirection = Vector3.Normalize(Vector3(-1.0f, 0.0f, -0.5f))
    }

type State =
    {
        ShowParameters: bool
        Exiting: bool
        Camera: FreeCamera
    }

let initialState content =
    let startingPosition = Vector3(0.5f * (content.MinX + content.MaxX), 0.5f * (content.MinY + content.MaxY), 1000.0f)
    { ShowParameters = false; Exiting = false; Camera = FreeCamera(startingPosition, 0.5f, 0.0f) }

let update (input: Input) gameContent (gameTime: GameTime) gameState =
    let time = float32 gameTime.TotalGameTime.TotalSeconds

    { gameState with
        ShowParameters = if input.JustPressed(Keys.P) then not gameState.ShowParameters else gameState.ShowParameters
        Exiting = input.Quit
        Camera = gameState.Camera.Updated(input, time)
    }

let showParameters gameContent gameState =
    let colour = Color.DarkSlateGray

    let height = gameContent.SpriteFont.MeasureString("Hello").Y

    gameContent.SpriteBatch.Begin()

    [
        "GeoVisualise"
        sprintf "LookDir X=%.3f" gameState.Camera.LookDirection.X
        sprintf "LookDir Y=%.3f" gameState.Camera.LookDirection.Y
        sprintf "LookDir Z=%.3f" gameState.Camera.LookDirection.Z
    ]
    |> List.iteri (fun i s -> gameContent.SpriteBatch.DrawString(gameContent.SpriteFont, s, new Vector2(0.0f, height * single i), colour))

    gameContent.SpriteBatch.End()

let draw (device: GraphicsDevice) gameContent gameState (gameTime: GameTime) =
    let time = (single gameTime.TotalGameTime.TotalMilliseconds) / 100.0f

    do device.Clear(Color.LightGray)

    gameContent.Effect.CurrentTechnique <- gameContent.Effect.Techniques.["Coloured"]

    gameContent.Effect.Parameters.["xWorld"].SetValue(gameContent.World)
    gameContent.Effect.Parameters.["xView"].SetValue(gameState.Camera.ViewMatrix)
    gameContent.Effect.Parameters.["xProjection"].SetValue(gameContent.Projection)
    gameContent.Effect.Parameters.["xLightDirection"].SetValue(gameContent.LightDirection)

    device.BlendState <- BlendState.Opaque
    device.DepthStencilState <- DepthStencilState.Default

    gameContent.Effect.CurrentTechnique.Passes |> Seq.iter
        (fun pass ->
            pass.Apply()
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, gameContent.Vertices, 0, gameContent.Vertices.Length, gameContent.Indices, 0, gameContent.Indices.Length / 3)
        )

    if gameState.ShowParameters then showParameters gameContent gameState
