﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaxManagedBridge
{
    public static class Defaults
    {
        public static bool  AmbientGammaCorrection  = true;
        public static bool  MapFilteringDisable     = true;
        public static int   MentalRay_AODistance    = 200;
        public static bool  MentalRay_AOEnable      = false;
        public static float MentalRay_GlossScalar   = 0.6f;
        public static float MentalRay_BumpScalar    = 1.2f;
        public static bool  MentalRay_EnableHighlightsFGOnly = true;
        public static float VRay_GlossScalar        = 1.0f;
        public static float VRay_BumpScalar         = 1.0f;
        public static float Standard_GlossScalar    = 1.0f;
        public static float Standard_BumpScalar     = 1.0f;
    }
}
