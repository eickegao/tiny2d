using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reflection;
using System.Xml;
using Tiny2d.Components;
using Tiny2d.SceneItems.ParticlesClasses;


namespace Tiny2d.SceneItems
{
    public interface IAnimationDirector
    {
        [CategoryAttribute("Animation Director"), DescriptionAttribute("Automatically play the default animation upon loading the SceneItem")]
        bool AutoPlay { get; set; }
        [CategoryAttribute("Animation Director"), DescriptionAttribute("Default animation name")]
        String DefaultAnimation { get; set; }
        [CategoryAttribute("Animation Director"), DescriptionAttribute("Current animation used by the Director")]
        IAnimation CurrentAnimation { get; }

        void SetAnimation(String animationName);
        void PlayAnimation(String animationName);
        void EnqueueAnimation(String animationName);
    }
}
