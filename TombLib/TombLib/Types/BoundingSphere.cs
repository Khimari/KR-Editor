﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TombLib
{
    public struct BoundingSphere : IEquatable<BoundingSphere>
    {
        public Vector3 Center;
        public float Radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public static BoundingSphere FromBoundingBox(BoundingBox bb)
        {
            var inR = Math.Max(Math.Max(bb.Size.X, bb.Size.Y), bb.Size.Z) * 0.5f;
            var outR = Vector3.Distance(bb.Minimum, bb.Maximum) * 0.5f;
            return new BoundingSphere(bb.Center, (inR + outR) * 0.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingSphere first, BoundingSphere second) => first.Center == second.Center && first.Radius == second.Radius;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingSphere first, BoundingSphere second) => first.Center != second.Center || first.Radius != second.Radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => "Sphere at " + Center + " with radius " + Radius;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => unchecked((Center.GetHashCode() * 397) ^ Radius.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BoundingSphere other)
        {
            return Center.Equals(other.Center) && Radius.Equals(other.Radius);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (!(other is BoundingSphere))
                return false;
            return Equals((BoundingSphere)other);
        }
    }
}
