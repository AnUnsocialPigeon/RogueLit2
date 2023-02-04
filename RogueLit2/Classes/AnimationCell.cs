namespace RogueLit2.Classes {
    internal class AnimationCell {
        internal AnimationCell() { }
        internal AnimationCell(float value, float accelRate, string icon) {
            Icon = icon;
            Value = value;
            AccelRate = accelRate;
        }

        internal string Icon = "▓▓";
        internal float Value { get => _value; set => _value = Math.Max(Math.Min(value, 255f), 0f); }
        private float _value = 0f;
        internal float AccelRate { get => _accelRate; set => _accelRate = Math.Max(Math.Min(value, 9f), 2f); }
        internal float _accelRate = 0f;
        internal bool Rise = false;
    }
}