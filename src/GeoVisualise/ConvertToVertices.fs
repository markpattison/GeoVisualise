module GeoVisualise.ConvertToVertices

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open ReadElevationData

let convert asc =
    let nCols = asc.NumCols
    let nRows = asc.NumRows
    let cellSize = single asc.CellSize
    let xBase = single asc.XllCorner + 0.5f * cellSize
    let yBase = single asc.YllCorner + 0.5f * cellSize

    let minX = xBase
    let maxX = xBase + cellSize * single (nCols - 1)
    let minY = yBase
    let maxY = yBase + cellSize * single (nRows - 1)

    let points = Array.init (nCols * nRows) (fun i ->
        let row = nCols - 1 - i / nCols
        let col = i % nCols

        let xf = single col
        let yf = single (i / nCols)
        if row = nCols - 1 && col = 0 then
            Vector3(xBase + xf * cellSize, yBase + yf * cellSize, 50.0f + single asc.Data.[row, col])
        else
            Vector3(xBase + xf * cellSize, yBase + yf * cellSize, single asc.Data.[row, col]))
    
    let normals : Vector3 [] = Array.zeroCreate (nCols * nRows)

    for x in [0 .. nCols - 2] do
        for y in [0 .. nRows - 2] do
            let indexBottomLeft  = x     + nCols * y
            let indexTopLeft     = x     + nCols * (y + 1)
            let indexBottomRight = x + 1 + nCols * y
            let indexTopRight    = x + 1 + nCols * (y + 1)

            let bottomLeft = points.[indexBottomLeft]
            let bottomRight = points.[indexBottomRight]
            let topLeft = points.[indexTopLeft]
            let topRight = points.[indexTopRight]

            let triangle1Normal = Vector3.Cross(bottomRight - bottomLeft, topLeft - bottomLeft) |> Vector3.Normalize

            normals.[indexBottomLeft] <- normals.[indexBottomLeft] + triangle1Normal
            normals.[indexTopLeft] <- normals.[indexTopLeft] + triangle1Normal
            normals.[indexBottomRight] <- normals.[indexBottomRight] + triangle1Normal

            let triangle2Normal = Vector3.Cross(topRight - topLeft, topRight - bottomRight) |> Vector3.Normalize

            normals.[indexBottomRight] <- normals.[indexBottomRight] + triangle2Normal
            normals.[indexTopLeft] <- normals.[indexTopLeft] + triangle2Normal
            normals.[indexTopRight] <- normals.[indexTopRight] + triangle2Normal

    let vertices = Array.init (nCols * nRows) (fun i ->
        let xTex = single (i % nCols) / single (nCols - 1)
        let yTex = single (nCols - 1 - (i / nCols)) / single (nRows - 1)
        VertexPositionNormalTexture(points.[i], Vector3.Normalize(normals.[i]), Vector2(xTex, yTex)))
    
    vertices, minX, maxX, minY, maxY

let indices nCols nRows =
    let indices =
        seq {
            for x in [0 .. nCols - 2] do
                for y in [0 .. nRows - 2] do
                    yield x     + nCols * y
                    yield x     + nCols * (y + 1)
                    yield x + 1 + nCols * y
                    yield x + 1 + nCols * y
                    yield x + nCols * (y + 1)
                    yield x + 1 + nCols * (y + 1)
        } |> Seq.toArray
    indices