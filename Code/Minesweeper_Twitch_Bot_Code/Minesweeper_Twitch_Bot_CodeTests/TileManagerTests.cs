using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests {
    [TestClass()]
    public class TileManagerTests {
        [TestMethod()]
        public void PlayerRevealRequestTest() {
            var TM = new TileManager();
            TM.PlayerRevealRequest(new Vector2(), "fart");
            var testInt = TM.testQueueNum;
        }

        [TestMethod()]
        public void GenerateGridTest() {
            var TM = new TileManager();
            TM.GenerateGrid(new Vector2(), "fart");
            var testInt = TM.testQueueNum;
        }
    }
}