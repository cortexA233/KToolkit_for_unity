using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    public static class GuidGenerator
    {
        public static string GenerateGuidString()
        {
            var newGuid = Guid.NewGuid();
            return newGuid.ToString();
        }
    }
}
