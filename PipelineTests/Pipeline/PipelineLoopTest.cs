using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Pipeline.Exceptions;

namespace PipelineTests.Pipeline
{
    [TestFixture]
    public class PipelineLoopTest
    {
        [Test]
        public void SimpleLoop()
        {
            // S -> LoopStart -> LoopEnd -> End

            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                dep.AddDependent(2, 0, 0);
                dep.AddDependent(2, 2, 1);
                nodes.Add(dep);
            }
            { // condition for end loop
                DependentNode dep = new DependentNode(5, "condition");
                dep.AddDependent(2, 1, 0);
                nodes.Add(dep);
            }
            { // loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(5, 0, 1);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(2, 0, 0);
                nodes.Add(dep);
            }

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<PipelineExecutor.LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(1, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
        }

        [Test]
        public void SimpleExtendedLoop()
        {
            // S -> LoopStart -> LoopEnd -> End

            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                dep.AddDependent(2, 0, 0);
                dep.AddDependent(2, 2, 1);
                nodes.Add(dep);
            }
            { // loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(4, 0, 0);
                nodes.Add(dep);
            }
            { // process node
                DependentNode dep = new DependentNode(4, "process");
                dep.AddDependency(2, 0, 0);
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(4, 0, 0);
                nodes.Add(dep);
            }

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<PipelineExecutor.LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(1, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
        }

        [Test]
        public void NoLoop()
        {
            // S -> -> End

            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(0, 0, 0);
                nodes.Add(dep);
            }

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<PipelineExecutor.LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(0, loops.Count, "incorrect amount of loops detected");
        }

        [Test]
        public void NestedLoop()
        {
            //          ᴧ  >>      >>      >>  v      
            // S -> LoopStart -> LoopEnd -> LoopEnd -> End
            //

            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                dep.AddDependent(2, 0, 0);
                dep.AddDependent(2, 2, 1);
                dep.AddDependent(4, 0, 0);
                dep.AddDependent(4, 2, 1);
                nodes.Add(dep);
            }
            { // condition for inner end loop
                DependentNode dep = new DependentNode(5, "condition");
                dep.AddDependent(2, 1, 0);
                nodes.Add(dep);
            }
            { // inner loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(5, 0, 1);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(4, 2, 0);
                nodes.Add(dep);
            }
            { // condition for outer end loop
                DependentNode dep = new DependentNode(6, "condition");
                dep.AddDependent(4, 1, 0);
                nodes.Add(dep);
            }
            { // outer loop end
                DependentNode dep = new DependentNode(4, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(6, 0, 1);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 0, 1);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(4, 0, 1);
                nodes.Add(dep);
            }

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<PipelineExecutor.LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(2, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
            Assert.AreEqual(1, loops[1].Depth);
            Assert.AreNotSame(loops[1].Id, loops[1].Id);
        }

        [Test]
        public void DuelLoops()
        {
            //                -> LoopEnd -> 
            // S -> LoopStart               End
            //                -> LoopEnd ->

            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                dep.AddDependent(2, 0, 0);
                dep.AddDependent(2, 2, 1);
                dep.AddDependent(4, 0, 0);
                dep.AddDependent(4, 2, 1);
                nodes.Add(dep);
            }
            { // condition for first end loop
                DependentNode dep = new DependentNode(5, "condition");
                dep.AddDependent(2, 1, 0);
                nodes.Add(dep);
            }
            { // first loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(5, 0, 1);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // condition for second end loop
                DependentNode dep = new DependentNode(6, "condition");
                dep.AddDependent(4, 1, 0);
                nodes.Add(dep);
            }
            { // second loop end
                DependentNode dep = new DependentNode(4, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(6, 0, 1);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 1, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(2, 0, 0);
                dep.AddDependency(4, 0, 1);
                nodes.Add(dep);
            }

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<PipelineExecutor.LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(2, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
            Assert.AreEqual(0, loops[1].Depth);
            Assert.AreNotSame(loops[1].Id, loops[1].Id);
        }

        [Test]
        public void MiddleExitLoops()
        {
            //                End
            //                 ᴧ
            // S -> LoopStart -> LoopEnd -> End

            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                dep.AddDependent(2, 0, 0);
                dep.AddDependent(2, 2, 1);
                dep.AddDependent(4, 1, 0);
                nodes.Add(dep);
            }
            { // condition for end loop
                DependentNode dep = new DependentNode(5, "condition");
                dep.AddDependent(2, 1, 0);
                nodes.Add(dep);
            }
            { // middle end
                DependentNode dep = new DependentNode(4, "end");
                dep.AddDependency(1, 1, 0);
                nodes.Add(dep);
            }
            { // loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(5, 0, 1);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(2, 0, 0);
                nodes.Add(dep);
            }

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<PipelineExecutor.LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(1, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
        }

        [Test]
        public void ImbalancedLoopLinkException()
        {
            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                //dep.AddDependent(2, 0, 0);
                dep.AddDependent(2, 2, 0);
                nodes.Add(dep);
            }
            { // first loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(2, 0, 0);
                nodes.Add(dep);
            }

            MissingLinkException exception = Assert.Throws<MissingLinkException>(() => new PipelineExecutor(ConvertToDictionary(nodes), 0));
            Assert.AreEqual("No link for loop start specified", exception.Message);
        }

        [Test]
        public void PartialLoopLinksException()
        {
            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                dep.AddDependent(2, 0, 0); // Loop start not linked to random end directly
                nodes.Add(dep);
            }
            { // first loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // random loop end
                DependentNode dep = new DependentNode(4, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 1, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(2, 0, 0);
                dep.AddDependency(4, 0, 1);
                nodes.Add(dep);
            }

            MissingLinkException exception = Assert.Throws<MissingLinkException>(() => new PipelineExecutor(ConvertToDictionary(nodes), 0));
            Assert.AreEqual("Loop start and loop end only partly referencing each other!", exception.Message);
        }

        [Test]
        public void InvalidLoopStartConnection()
        {
            List<DependentNode> nodes = new List<DependentNode>();
            { // start node
                DependentNode dep = new DependentNode(0, "start");
                dep.AddDependent(1, 0, 0);
                nodes.Add(dep);
            }
            { // loop start
                DependentNode dep = new DependentNode(1, LoopStart.TypeName);
                dep.AddDependency(0, 0, 0);
                dep.AddDependent(2, 0, 0);
                dep.AddDependent(3, 0, 0); //Can't connect to non Loop End node
                dep.AddDependent(2, 2, 1);
                nodes.Add(dep);
            }
            { // condition for end loop
                DependentNode dep = new DependentNode(5, "condition");
                dep.AddDependent(2, 1, 0);
                nodes.Add(dep);
            }
            { // loop end
                DependentNode dep = new DependentNode(2, LoopEnd.TypeName);
                dep.AddDependency(1, 0, 0);
                dep.AddDependency(5, 0, 1);
                dep.AddDependency(1, 1, 2);
                dep.AddDependent(3, 0, 0);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(1, 0, 1);
                dep.AddDependency(2, 0, 0);
                nodes.Add(dep);
            }

            InvalidNodeException exception = Assert.Throws<InvalidNodeException>(() => new PipelineExecutor(ConvertToDictionary(nodes), 0));
            Assert.AreEqual("Loop Start Link (slot 0) cannot link to anything but a Loop End", exception.Message);
        }

        private Dictionary<int, DependentNode> ConvertToDictionary(List<DependentNode> deps)
        {
            Dictionary<int, DependentNode> dependent = new Dictionary<int, DependentNode>();

            foreach (DependentNode dep in deps)
                dependent.Add(dep.Id, dep);

            return dependent;
        }
    }
}
