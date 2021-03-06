﻿using System;
using UnityEngine;
using System.Collections; using FastCollections;
using Newtonsoft.Json;
using Pathfinding;

namespace Lockstep
{
    //THIS IS HAPPENING!!!!
    [System.Serializable]
    public struct Vector3d : Lockstep.ICommandData
    {
        [FixedNumber]
		/// <summary>
		/// Horizontal
		/// </summary>
        public long x;
        [FixedNumber]
		/// <summary>
		/// Forward/Backward
		/// </summary>
        public long y;
        [FixedNumber]
		/// <summary>
		/// Up/Down
		/// </summary>
        public long z; //Height

        public long magnitude
        {
            get
            {
                return FixedMath.Sqrt(this.sqrMagnitude);
            }
        }

        internal static Vector3d zero = new Vector3d(0,0,0);
        internal static Vector3d up = new Vector3d(0, FixedMath.One, 0);
        internal static Vector3d down = new Vector3d(0, -FixedMath.One, 0);
        public Vector3d (Vector3 vec3) {
            this.x = FixedMath.Create(vec3.x);
            this.y = FixedMath.Create(vec3.y);
            this.z = FixedMath.Create(vec3.z);
        }
        public Vector3d (Vector2d vec) {
            this.x = vec.x;
            this.y = 0;
            this.z = vec.y;
        }
        public Vector3d (long X, long Y, long Z) {
            x = X;
            y = Y;
            z = Z;
        }
        public static Vector3d GetForward()
        {
            return new Vector3d(0, 0, FixedMath.One);
        }
        public static Vector3d Normalize(Vector3d v)
        {
            long magnitude = FixedMath.Sqrt(v.x.Mul(v.x) + v.y.Mul(v.y) + v.z.Mul(v.z));
            if (magnitude == 0)
                return Vector3d.zero;
            return new Vector3d(v.x.Div(magnitude), v.y.Div(magnitude), v.z.Div(magnitude));
        }
        public Vector3d Normalize () {
            long magnitude = FixedMath.Sqrt(x.Mul(x) + y.Mul(y) + z.Mul(z));
            if (magnitude == 0)
                return Vector3d.zero;
            return new Vector3d(x.Div(magnitude), y.Div(magnitude), z.Div(magnitude));
        }

        public Vector3d Scale(Vector3d v)
        {
            this.x = this.x.Mul(v.x);
            this.y = this.x.Mul(v.y);
            this.z = this.x.Mul(v.z);
            return this;
        }
        public Vector2d ToVector2d () {
            return new Vector2d(x,z);
        }
        public Vector3 ToVector3 () {
            return new Vector3(x.ToFloat(),y.ToFloat(),z.ToFloat());
        }
        public void SetVector2d(Vector2d vec2d) {
            x = vec2d.x;
            y = vec2d.y;
        }
        public void Add (ref Vector2d other) {
            x += other.x;
            y += other.y;
        }
        public void Add (ref Vector3d other) {
            x += other.x;
            y += other.y;
            z += other.z;
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            Vector3d v = a;
            v.Add(b);
            return v;
        }
        public Vector3d Add (Vector3d other) {
            x += other.x;
            y += other.y;
            z += other.z;
            return this;
        }
        public Vector3d Mul (long f1) {
            x *= f1;
            x >>= FixedMath.SHIFT_AMOUNT;
            y *= f1;
            y >>= FixedMath.SHIFT_AMOUNT;
            z *= f1;
            z >>= FixedMath.SHIFT_AMOUNT;
            return this;
        }
        public Vector3d Mul (int i) {
            x *= i;
            y *= i;
            z *= i;
            return this;
        }
        public Vector3d Div(int i)
        {
            x /= i;
            y /= i;
            z /= i;
            return this;
        }
        public Vector3d Div(long f1)
        {
            x = x.Div(f1);
            y = y.Div(f1);
            z = z.Div(f1);
            return this;
        }
        public static Vector3d operator *(Vector3d a, long b)
        {
            Vector3d v = a;
            v.Mul(b);
            return v;
        }
        public static Vector3d operator /(Vector3d a, long b)
        {
            Vector3d v = a;
            v.Div(b);
            return v;
        }
        public static Vector3d operator *(Vector3d a, int b)
        {
            Vector3d v = a;
            v.Mul(b);
            return v;
        }
        public static Vector3d operator /(Vector3d a, int b)
        {
            Vector3d v = a;
            v.Div(b);
            return v;
        }
        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            Vector3d v = a;
            int nagtive = -1;
            v.Add(b*nagtive);
            return v;
        }
        public static Vector3d ClampMagnitude(Vector3d vector, long maxLength)
        {
            if (vector.sqrMagnitude > maxLength.Mul(maxLength))
                return vector.Normalize() * maxLength;
            return vector;
        }

        [JsonIgnore]
        public long sqrMagnitude {
            get { return this.x.Mul(x) + this.y.Mul(y) + this.z.Mul(z); }
        }

        public static bool operator ==(Vector3d a, Vector3d b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }
        public static bool operator !=(Vector3d a, Vector3d b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }
        public static Vector3d operator -(Vector3d a)
        {
            return a * -1;
        }
        public static long Distance(Vector3d a, Vector3d b)
        {
            long tX = b.x - a.x;
            tX *= tX;
            tX >>= FixedMath.SHIFT_AMOUNT;
            long tY = b.y - a.y;
            tY *= tY;
            tY >>= FixedMath.SHIFT_AMOUNT;
            long tZ = b.z - a.z;
            tZ *= tZ;
            tZ >>= FixedMath.SHIFT_AMOUNT;
            return FixedMath.Sqrt(tX + tY + tZ);

        }

        public static long SqrDistance(Vector3d a, Vector3d b)
        {
            long tX = b.x - a.x;
            tX *= tX;
            tX >>= FixedMath.SHIFT_AMOUNT;
            long tY = b.y - a.y;
            tY *= tY;
            tY >>= FixedMath.SHIFT_AMOUNT;
            long tZ = b.z - a.z;
            tZ *= tZ;
            tZ >>= FixedMath.SHIFT_AMOUNT;
            return tX + tY + tZ;
        }
        public long Distance (Vector3d other) {
            long tX = other.x - x;
            tX *= tX;
            tX >>= FixedMath.SHIFT_AMOUNT;
            long tY = other.y - y;
            tY *= tY;
            tY >>= FixedMath.SHIFT_AMOUNT;
            long tZ = other.z - z;
            tZ *= tZ;
            tZ >>= FixedMath.SHIFT_AMOUNT;
            return FixedMath.Sqrt(tX + tY + tZ);
        }

        public static Int3 ToInt3(Vector3d v)
        {
            return new Int3((v.x*1000).ToInt(), (v.y * 1000).ToInt(), (v.z * 1000).ToInt());
        }
        public static long Dot(Vector3d a, Vector3d b)
        {
            return a.x.Mul(b.x) + a.y.Mul(b.y) + a.z.Mul(b.z);
        }

        public static Vector3d Cross(Vector3d lhs, Vector3d rhs)
        {
            return new Vector3d((lhs.y .Mul( rhs.z) - lhs.z.Mul(rhs.y)), (lhs.z.Mul(rhs.x) - lhs.x.Mul(rhs.z)), (lhs.x.Mul(rhs.y) - lhs.y.Mul(rhs.x)));
        }
        [JsonIgnore]
        public long LongStateHash {get {return (x * 31 + y * 7 + z * 11);}}
        [JsonIgnore]
        public int StateHash {get {return (int)(LongStateHash % int.MaxValue);}}

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x.ToFormattedDouble().ToString("0.0000"),y.ToFormattedDouble().ToString("0.0000"), z.ToFormattedDouble());
        }

        public string ToStringRaw()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
        public void Write (Writer writer) {
            writer.Write(x);
            writer.Write(y);
            writer.Write(z);
        }

        public void Read (Reader reader) {
            x = reader.ReadLong();
            y = reader.ReadLong();
            z = reader.ReadLong();
        }
    }
}