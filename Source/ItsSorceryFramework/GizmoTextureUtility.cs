using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace ItsSorceryFramework
{
    [StaticConstructorOnStartup]
    public static class GizmoTextureUtility
    {

        public static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        public static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(.518f, .427f, .239f)); //RGB #846D3D | dull gold (normal qi)

        public static readonly Texture2D OverBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.0f, 0.6588f, 0.4196f)); //RGB #00A86B | jade (qi overcharge)

        public static readonly Texture2D UnderBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.6f, 0f, 0f)); //RGB #990000 | crimson red (qi deprivation)

        public static readonly Texture2D PauseButton = ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause", true);

        public static readonly Texture2D PlayButton = ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Normal", true);
    }
}
