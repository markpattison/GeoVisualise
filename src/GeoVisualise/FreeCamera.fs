module GeoVisualise.FreeCamera

open Microsoft.Xna.Framework

open Input

let maxLookUpDown = 1.5f;
let rotSpeed = 0.00005f;            // per millisecond
let moveSpeed = 10.0f;
let upDirection = Vector3(0.0f, 0.0f, 1.0f)

// members
type FreeCamera(position: Vector3,
                lookAroundX: single,
                lookAroundZ: single) =
    let rotZ = Matrix.CreateRotationZ(-lookAroundZ)
    let lookDirection = 
            let rot1 = Matrix.CreateRotationX(-lookAroundX)
            let combined = Matrix.Multiply(rot1, rotZ)
            let temp2 = Vector3.Transform(Vector3.UnitY, combined)
            temp2.Normalize()
            temp2
    let rightDirection =
            let temp2 = Vector3.Transform(Vector3.UnitX, rotZ)
            temp2.Normalize()
            temp2
    let lookAt = position + lookDirection
    member _this.ViewMatrix = Matrix.CreateLookAt(position, lookAt, upDirection)
    member _this.Position = position
    member _this.LookAroundX = lookAroundX
    member _this.LookAroundY = lookAroundZ
    member _this.LookAt = lookAt
    member _this.LookDirection = lookDirection
    member _this.RightDirection = rightDirection
    member _this.Updated(input : Input, t) =
        let mutable newPosition = position
        if input.Left then newPosition <- newPosition - rightDirection * moveSpeed
        if input.Right then newPosition <- newPosition + rightDirection * moveSpeed
        if input.Up then newPosition <- newPosition + upDirection * moveSpeed
        if input.Down then newPosition <- newPosition - upDirection * moveSpeed
        if input.Forward then newPosition <- newPosition + lookDirection * moveSpeed
        if input.Backward then newPosition <- newPosition - lookDirection * moveSpeed
        let newLookAroundZ = lookAroundZ + rotSpeed * t * single input.MouseDX
        let newLookAroundX = MathHelper.Clamp(lookAroundX + rotSpeed * t * single input.MouseDY, -maxLookUpDown, maxLookUpDown)
        FreeCamera(newPosition, newLookAroundX, newLookAroundZ)
