using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace refw.BT {
    public class Monitor: Parallel {
        private new Policy FailurePolicy;
        private new Policy SuccessPolicy;

        public Monitor() {
            FailurePolicy = Policy.One;
            SuccessPolicy = Policy.All;
        }
    }
}
