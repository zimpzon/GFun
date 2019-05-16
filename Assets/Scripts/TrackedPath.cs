using System.Text;
using UnityEngine;

namespace GFun
{
    public struct TrackedPathSample
    {
        public int x;
        public int y;
        public int t;
    }

    public class TrackedPath
    {
        public const int MaxPathLen = 100;

        const float Resolution = 100;
        static readonly StringBuilder SB = new StringBuilder((int)Resolution * 20);

        TrackedPathSample[] path_ = new TrackedPathSample[MaxPathLen];
        int idx_;

        int baseX_;
        int baseY_;
        int baseT_;
        float fullBaseX_;
        float fullBaseY_;
        float fullBaseT_;
        Vector3 latestMoveVec_;

        public void Rewind() => idx_ = 0;

        public float EndT => idx_ == 0 ? 0 : path_[idx_ - 1].t / Resolution;

        public bool HasPath => idx_ != 0;

        public Vector3 GetPosAtTime(float t)
        {
            float endT = EndT;
            if (t >= endT)
                return GetPosAtIdx(idx_ - 1);

            int startIdx = 0;
            while (t > path_[startIdx + 1].t / Resolution)
                startIdx++;

            var p0 = GetPosAtIdx(startIdx);
            var p1 = GetPosAtIdx(startIdx + 1);

            float seg0T = path_[startIdx].t / Resolution;
            float seg1T = path_[startIdx + 1].t / Resolution;
            float segmentTime = seg1T - seg0T;
            float segmentT = (t - seg0T) / segmentTime;
            return Vector3.Lerp(p0, p1, segmentT);
        }

        public Vector3 GetPosAtIdx(int idx)
            => new Vector3(path_[idx].x / Resolution + fullBaseX_, path_[idx].y / Resolution + fullBaseY_, 0);

        bool TryGetInt(string s, ref int pos, out int value)
        {
            value = 0;
            if (pos >= s.Length)
                return false;

            bool isNegative = s[pos] == '-';
            if (isNegative)
                pos++;

            int idxFirst = pos;
            while (char.IsNumber(s, pos))
                pos++;

            int idxEnd = pos;
            int exp = 1;
            for (int i = idxEnd - 1; i >= idxFirst; --i)
            {
                value += (s[i] - '0') * exp;
                exp *= 10;
            }

            if (isNegative)
                value *= -1;

            // Skip trailing ';'
            pos++;
            return true;
        }

        public void FromString(string path)
        {
            Rewind();
            if (string.IsNullOrWhiteSpace(path))
                return;

            int pos = 0;
            TryGetInt(path, ref pos, out baseX_);
            TryGetInt(path, ref pos, out baseY_);
            TryGetInt(path, ref pos, out baseT_);
            fullBaseX_ = baseX_ / Resolution;
            fullBaseY_ = baseY_ / Resolution;
            fullBaseT_ = baseT_ / Resolution;

            idx_ = 0;
            while (TryGetInt(path, ref pos, out int x) && idx_ < MaxPathLen - 1)
            {
                TryGetInt(path, ref pos, out int y);
                TryGetInt(path, ref pos, out int t);
                path_[idx_++] = new TrackedPathSample { x = x, y = y, t = t};
            }
        }

        public string AsString()
        {
            SB.Clear();
            if (idx_ > 0)
            {
                SB.AppendFormat($"{baseX_};{baseY_};{baseT_};");
                for (int i = 0; i < idx_; ++i)
                {
                    SB.AppendFormat($"{path_[i].x};{path_[i].y};{path_[i].t};");
                }
            }
            return SB.ToString();
        }

        public void DebugDraw()
        {
            for (int i = 1; i < idx_; ++i)
            {
                var pos0 = GetPosAtIdx(i - 1);
                var pos1 = GetPosAtIdx(i);
                Debug.DrawLine(pos0, pos1, Color.white);
            }
        }

        public bool Sample(Vector3 moveVec, Vector3 pos, float t)
        {
            if (idx_ >= MaxPathLen)
                return false;

            if (moveVec != latestMoveVec_)
            {
                if (idx_ == 0)
                {
                    baseX_ = (int)(pos.x * Resolution);
                    baseY_ = (int)(pos.y * Resolution);
                    baseT_ = (int)(Time.unscaledTime * Resolution);
                    fullBaseX_ = baseX_ / Resolution;
                    fullBaseY_ = baseY_ / Resolution;
                    fullBaseT_ = baseT_ / Resolution;
                }

                bool isResumingMovement = latestMoveVec_ == Vector3.zero && idx_ != 0;
                if (isResumingMovement && idx_ < MaxPathLen - 2)
                {
                    // Was standing still, but is now moving again. Add and end-sample for the time we were standing still.
                    // If we don't do this the time we should stgand still is actually slowly interpolating to the first
                    // position after the movement.
                    var standingStill = path_[idx_ - 1];
                    standingStill.t = (int)(t * Resolution - baseT_);
                    path_[idx_++] = standingStill;
                }

                latestMoveVec_ = moveVec;
                var sample = new TrackedPathSample
                {
                    x = (int)(pos.x * Resolution - baseX_),
                    y = (int)(pos.y * Resolution - baseY_),
                    t = (int)(t * Resolution - baseT_),
                };
                path_[idx_++] = sample;
            }

            return true;
        }
    }
}
