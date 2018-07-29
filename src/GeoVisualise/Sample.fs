module GeoVisualise.Sample

open System.IO

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

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

        SpotHeightVertices: VertexPositionNormalTexture []
        ContourVertices: VertexPosition [][]

        DrawDebug: Texture2D -> unit
    }

let createDebug (device: GraphicsDevice) (effect: Effect) =
    let debugVertices =
        [|
            VertexPositionTexture(Vector3(-0.9f, 0.5f, 0.0f), new Vector2(0.0f, 1.0f));
            VertexPositionTexture(Vector3(-0.9f, 0.9f, 0.0f), new Vector2(0.0f, 0.0f));
            VertexPositionTexture(Vector3(-0.5f, 0.5f, 0.0f), new Vector2(1.0f, 1.0f));

            VertexPositionTexture(Vector3(-0.5f, 0.5f, 0.0f), new Vector2(1.0f, 1.0f));
            VertexPositionTexture(Vector3(-0.9f, 0.9f, 0.0f), new Vector2(0.0f, 0.0f));
            VertexPositionTexture(Vector3(-0.5f, 0.9f, 0.0f), new Vector2(1.0f, 0.0f));
        |]
    
    let drawDebug (texture: Texture2D) =
        effect.CurrentTechnique <- effect.Techniques.["Debug"]
        effect.Parameters.["xDebugTexture"].SetValue(texture)

        effect.CurrentTechnique.Passes |> Seq.iter
            (fun pass ->
                pass.Apply()
                device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, debugVertices, 0, debugVertices.Length / 3)
            )
    
    drawDebug

let loadContent (_this: Game) device (graphics: GraphicsDeviceManager) =
    let ascStream = new StreamReader(@"data\TL11\TL11.asc")
    let data = ReadElevationData.readAsc ascStream
    let vertices, minX, maxX, minY, maxY = ConvertToVertices.convert data
    let indices = ConvertToVertices.indices data.NumCols data.NumRows

    let contourData = ReadContourData.readContours @"data\TL11\TL11.gml"
    let spotHeightVertices = CreateContourVertices.createSpotHeightVertices contourData.SpotHeights
    let contourVertices = CreateContourVertices.createAllContourVertices contourData.Contours

    let effect = _this.Content.Load<Effect>("Effects/effects")

    {
        Effect = effect
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

        SpotHeightVertices = spotHeightVertices
        ContourVertices = contourVertices

        DrawDebug = createDebug device effect
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

    gameContent.Effect.Parameters.["xWorld"].SetValue(gameContent.World)
    gameContent.Effect.Parameters.["xView"].SetValue(gameState.Camera.ViewMatrix)
    gameContent.Effect.Parameters.["xProjection"].SetValue(gameContent.Projection)
    gameContent.Effect.Parameters.["xLightDirection"].SetValue(gameContent.LightDirection)
    gameContent.Effect.Parameters.["xSpotHeightColour"].SetValue(Color.Yellow.ToVector4())
    gameContent.Effect.Parameters.["xTerrainColour"].SetValue(Color.Green.ToVector4())
    gameContent.Effect.Parameters.["xContourColour"].SetValue(Color.Black.ToVector4())

    // draw terrain

    gameContent.Effect.CurrentTechnique <- gameContent.Effect.Techniques.["Terrain"]

    device.BlendState <- BlendState.Opaque
    device.DepthStencilState <- DepthStencilState.Default

    gameContent.Effect.CurrentTechnique.Passes |> Seq.iter
        (fun pass ->
            pass.Apply()
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, gameContent.Vertices, 0, gameContent.Vertices.Length, gameContent.Indices, 0, gameContent.Indices.Length / 3)
        )

    // draw contours

    gameContent.Effect.CurrentTechnique <- gameContent.Effect.Techniques.["Contour"]

    let rs = new RasterizerState()
    rs.DepthBias <- 10.0f;

    device.RasterizerState <- rs

    gameContent.Effect.CurrentTechnique.Passes |> Seq.iter
        (fun pass ->
            pass.Apply()
            gameContent.ContourVertices
            |> Array.iter (fun contour ->
                device.DrawUserPrimitives(PrimitiveType.LineStrip, contour, 0, contour.Length - 1))
        )

    // draw spot heights

    gameContent.Effect.CurrentTechnique <- gameContent.Effect.Techniques.["SpotHeight"]

    device.BlendState <- BlendState.Opaque
    device.DepthStencilState <- DepthStencilState.Default

    gameContent.Effect.CurrentTechnique.Passes |> Seq.iter
        (fun pass ->
            pass.Apply()
            device.DrawUserPrimitives(PrimitiveType.TriangleList, gameContent.SpotHeightVertices, 0, gameContent.SpotHeightVertices.Length / 3)
        )
    
    // draw parameters

    if gameState.ShowParameters then showParameters gameContent gameState
