﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIO.Framework.Extensions.Standard.Database.InputDatabase;
using BIO.Framework.Extensions.Emgu.InputData;

namespace BIO.Project.IrisRecognition
{
    class ProjectSettings :
        ProjectSettings<StandardRecord<StandardRecordData>, EmguGrayImageInputData>,
        IStandardProjectSettings<StandardRecord<StandardRecordData>> {

        public int TemplateSamples {
            get {
                return 1;
            }
        }

        public override Framework.Core.Database.IDatabaseCreator<StandardRecord<StandardRecordData>> getDatabaseCreator() {
            return new IrisDatabaseCreator(@"d:\db\face\2D\JAFFE");
        }

        protected override Framework.Core.Evaluation.Block.IBlockEvaluatorSettings<StandardRecord<StandardRecordData>, EmguGrayImageInputData> getEvaluatorSettings() {
            return new IrisEvaluationSettings();
        }
    }
}
