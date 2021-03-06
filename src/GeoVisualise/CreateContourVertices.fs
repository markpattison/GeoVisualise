﻿module GeoVisualise.CreateContourVertices

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open ReadContourData

let width = 20.0f
let height = 20.0f
let offset = 30.0f

let calculateNormal v1 v2 v3 =
    Vector3.Cross(v3 - v1, v2 - v1)
    |> Vector3.Normalize

let createVerticesForSpotHeight spotHeight =
    let v1   = Vector3(single spotHeight.SpotX, single spotHeight.SpotY + width, single spotHeight.Elevation + offset)
    let v2   = Vector3(single spotHeight.SpotX + 0.866f * width, single spotHeight.SpotY - 0.5f * width, single spotHeight.Elevation + offset)
    let v3   = Vector3(single spotHeight.SpotX - 0.866f * width, single spotHeight.SpotY - 0.5f * width, single spotHeight.Elevation + offset)
    let vTop = Vector3(single spotHeight.SpotX, single spotHeight.SpotY, single spotHeight.Elevation + offset + height)

    let normal1 = calculateNormal v1 v2 vTop
    let normal2 = calculateNormal v2 v3 vTop
    let normal3 = calculateNormal v3 v1 vTop

    [|
        VertexPositionNormalTexture(v1  , normal1, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(v2  , normal1, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(vTop, normal1, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(v2  , normal2, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(v3  , normal2, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(vTop, normal2, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(v3  , normal3, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(v1  , normal3, Vector2(1.0f, 0.0f))
        VertexPositionNormalTexture(vTop, normal3, Vector2(1.0f, 0.0f))
    |]

let createSpotHeightVertices spotHeights =
    spotHeights
    |> Array.collect createVerticesForSpotHeight

let createContourVertices contour =
    contour.Points
    |> Array.map (fun p -> VertexPosition(Vector3(single p.X, single p.Y, single contour.Height)))

let createAllContourVertices contours =
    contours
    |> Array.map createContourVertices