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
            List<LoopPair> loops = pipe.getLoops();

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
            List<LoopPair> loops = pipe.getLoops();

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
            List<LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(0, loops.Count, "incorrect amount of loops detected");
        }

        [Test]
        public void NestedLoop()
        {
            //          ᴧ  >>      >>      >>  v      
            // S -> LoopStart -> LoopEnd -> LoopEnd -> End
            // 0        1           2          4        3

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
                dep.AddDependent(4, 3, 0);
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
                dep.AddDependency(1, 1, 2);
                dep.AddDependency(2, 0, 3);
                dep.AddDependency(6, 0, 1);
                dep.AddDependent(3, 0, 1);
                nodes.Add(dep);
            }
            { // end node
                DependentNode dep = new DependentNode(3, "end");
                dep.AddDependency(4, 0, 1);
                nodes.Add(dep);
            }

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(2, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
            Assert.AreEqual(1, loops[1].Depth);
            Assert.AreNotSame(loops[1].Id, loops[0].Id);
        }
        [Test]
        public void NestedLoop2()
        {
            // S -> LoopStart -> LoopStart -> Process -> LoopEnd -> LoopEnd -> End

            List<DependentNode> nodes = new List<DependentNode>();
            DependentNode start = new DependentNode(0, "start"),
                outerLoopStart = new DependentNode(1, LoopStart.TypeName),
                innerLoopStart = new DependentNode(2, LoopStart.TypeName),
                process = new DependentNode(3, ""),
                innerLoopEnd = new DependentNode(4, LoopEnd.TypeName),
                outerLoopEnd = new DependentNode(5, LoopEnd.TypeName),
                end = new DependentNode(6, "end");

            nodes.Add(start);
            nodes.Add(outerLoopEnd);
            nodes.Add(innerLoopEnd);
            nodes.Add(innerLoopStart);
            nodes.Add(outerLoopStart);
            nodes.Add(process);
            nodes.Add(end);

            MatchSlots(start, outerLoopStart, 0, 0);
            MatchSlots(outerLoopStart, outerLoopEnd, 0, 0);
            MatchSlots(innerLoopStart, innerLoopEnd, 0, 0);
            MatchSlots(innerLoopStart, process, 1, 0);
            MatchSlots(innerLoopEnd, outerLoopEnd, 1, 1);
            MatchSlots(process, innerLoopEnd, 0, 1);
            MatchSlots(outerLoopEnd, end, 0, 0);

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(2, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(1, loops[0].Depth);
            Assert.AreEqual(0, loops[1].Depth);
            Assert.AreNotSame(loops[0].Id, loops[1].Id);
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
            List<LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(2, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
            Assert.AreEqual(0, loops[1].Depth);
            Assert.AreNotSame(loops[1].Id, loops[1].Id);
        }

        [Test]
        public void SplitLoops()
        {
            //                -> LoopEnd -> End
            // S -> LoopStart 
            //                -> LoopEnd -> End

            List<DependentNode> nodes = new List<DependentNode>();

            DependentNode start = new DependentNode(0, "start"),
                loopStart = new DependentNode(1, LoopStart.TypeName),
                loopEnd1 = new DependentNode(2, LoopEnd.TypeName),
                loopEnd2 = new DependentNode(3, LoopEnd.TypeName),
                end1 = new DependentNode(4, "end"),
                end2 = new DependentNode(5, "end");

            nodes.Add(start);
            nodes.Add(loopStart);
            nodes.Add(loopEnd1);
            nodes.Add(loopEnd2);
            nodes.Add(end1);
            nodes.Add(end2);

            MatchSlots(start, loopStart, 0, 0);
            MatchSlots(loopStart, loopEnd1, 0, 0);
            MatchSlots(loopStart, loopEnd1, 1, 2);
            MatchSlots(loopEnd1, end1, 0, 0);
            MatchSlots(loopStart, loopEnd2, 0, 0);
            MatchSlots(loopStart, loopEnd2, 1, 2);
            MatchSlots(loopEnd2, end2, 0, 0);

            PipelineExecutor pipe = new PipelineExecutor(ConvertToDictionary(nodes), 0);
            List<LoopPair> loops = pipe.getLoops();

            Assert.AreEqual(2, loops.Count, "incorrect amount of loops detected");
            Assert.AreEqual(0, loops[0].Depth);
            Assert.AreEqual(0, loops[1].Depth);
            Assert.AreNotSame(loops[0].Id, loops[1].Id);
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
            List<LoopPair> loops = pipe.getLoops();

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

        //helpers

        private Dictionary<int, DependentNode> ConvertToDictionary(List<DependentNode> deps)
        {
            Dictionary<int, DependentNode> dependent = new Dictionary<int, DependentNode>();

            foreach (DependentNode dep in deps)
                dependent.Add(dep.Id, dep);

            return dependent;
        }

        private void MatchSlots(DependentNode a, DependentNode b, int aSlot, int bSlot)
        {
            a.AddDependent(b.Id, bSlot, aSlot);
            b.AddDependency(a.Id, aSlot, bSlot);
        }
    }
}
