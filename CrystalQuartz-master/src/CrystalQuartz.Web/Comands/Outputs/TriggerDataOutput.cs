﻿namespace CrystalQuartz.Web.Comands.Outputs
{
    using CrystalQuartz.Core.Domain;

    public class TriggerDataOutput : CommandResultWithErrorDetails
    {
         public TriggerData Trigger { get; set; }
    }
}