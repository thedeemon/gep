using System;
using System.Collections.Generic;
using System.Text;

namespace gep
{
    abstract class Animated
    {
        protected abstract bool IsSelected();
        protected abstract void Redraw();
        protected abstract bool IsHovered();

        DateTime animation_start_time = DateTime.MinValue;
        AnimationDirection animation_dir = AnimationDirection.None;
        protected int animation_state = 0;

        public bool OnMouseOver(DateTime t) //true if must be added to animations list
        {
            if (IsSelected()) return false;
            switch (animation_dir)
            {
                case AnimationDirection.None:
                    animation_dir = AnimationDirection.Up;
                    animation_state = 0;
                    animation_start_time = t;
                    break;
                case AnimationDirection.Up:
                    break;
                case AnimationDirection.Stay:
                    break;
                case AnimationDirection.Down:
                    animation_dir = AnimationDirection.Up;
                    animation_start_time = t;
                    break;
            }
            return true;
        }

        public bool Animate(DateTime t) // true if keep in animations list
        {
            if (IsSelected())
            {
                animation_state = 0;
                animation_dir = AnimationDirection.None;
                return false;
            }

            double anim_dur = 0.2; //seconds
            bool redraw = false;
            bool keep_anim = false;
            double dt = Math.Max((t - animation_start_time).TotalSeconds, 0.0);
            switch (animation_dir)
            {
                case AnimationDirection.None:
                    keep_anim = false;
                    break;
                case AnimationDirection.Up:
                    if (dt <= anim_dur)
                    {
                        animation_state = (int)(dt / anim_dur * 100);
                        keep_anim = true;
                        redraw = true;
                    }
                    else
                    {
                        animation_state = 100;
                        animation_dir = AnimationDirection.Stay;
                        keep_anim = true;
                        redraw = true;
                    }
                    break;
                case AnimationDirection.Stay:
                    if (IsHovered())
                    {
                        animation_state = 100;
                        keep_anim = true;
                    }
                    else
                    {
                        animation_dir = AnimationDirection.Down;
                        animation_start_time = t;
                        keep_anim = true;
                    }
                    break;
                case AnimationDirection.Down:
                    if (dt <= anim_dur)
                    {
                        animation_state = 100 - (int)(dt / anim_dur * 100);
                        keep_anim = true;
                        redraw = true;
                    }
                    else
                    {
                        animation_state = 0;
                        animation_dir = AnimationDirection.None;
                        keep_anim = false;
                        redraw = true;
                    }
                    break;
            }

            if (redraw)
                Redraw();

            return keep_anim;
        }
    }

    enum AnimationDirection
    {
        None, Up, Stay, Down
    }
}
