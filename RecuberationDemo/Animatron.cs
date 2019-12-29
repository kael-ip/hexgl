using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTex.Recuberation {

    class Tracker {
        public abstract class TrackerCommand {
            public abstract void Execute(Tracker tracker);
        }
        public class CommandDelay : TrackerCommand {
            public int Delay { get; private set; }
            public CommandDelay(int delay) {
                this.Delay = delay;
            }
            public override void Execute(Tracker tracker) {
                tracker.rowCounter = Delay;
            }
        }
        public class CommandCall : TrackerCommand {
            private Action<Tracker> callback;
            public CommandCall(Action<Tracker> callback) {
                this.callback = callback;
            }
            public override void Execute(Tracker tracker) {
                callback.Invoke(tracker);
            }
        }
        public class CommandLabel : TrackerCommand {
            public CommandLabel() { }
            public override void Execute(Tracker tracker) {
                tracker.labels.Push(tracker.pc);
                tracker.labels.Push(0);
            }
        }
        public class CommandLoop : TrackerCommand {
            private int delta, limit;
            public CommandLoop(int delta, int limit) {
                this.delta = delta;
                this.limit = limit;
            }
            public override void Execute(Tracker tracker) {
                int counter = tracker.labels.Pop();
                if(counter == limit) {
                    tracker.pc = tracker.labels.Pop();
                } else {
                    tracker.pc = tracker.labels.Peek();
                    counter += delta;
                    tracker.labels.Push(counter);
                }
            }
        }
        public int RowRate;
        private int frameCounter;
        private int rowCounter;
        public int Frame { get; private set; }
        private List<TrackerCommand> program = new List<TrackerCommand>();
        private int pc;
        private Stack<int> labels = new Stack<int>();
        public Action<Tracker> FrameHandler;
        public void Add(TrackerCommand command) {
            program.Add(command);
        }
        public void Reset() {
            frameCounter = 0;
            rowCounter = 0;
            labels.Clear();
            pc = 0;
        }
        public void Advance() {
            frameCounter--;
            if(frameCounter <= 0) {
                rowCounter--;
                if(rowCounter <= 0) {
                    rowCounter = 0;
                    while(rowCounter == 0 && pc < program.Count) {
                        var command = program[pc++];
                        command.Execute(this);
                    }
                }
                frameCounter = RowRate;
            }
            if(FrameHandler != null)
                FrameHandler.Invoke(this);
            Frame++;
        }
    }
}
