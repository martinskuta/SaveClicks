#region Usings

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using NUnit.Framework;
using Rhino.Mocks;
using SaveClicks.Services.CommandService.Impl;
using Skuta.Testing;
using Command = SaveClicks.Services.CommandService.Command;

#endregion

namespace SaveClicks.Tests.Services.CommandService.Impl
{
    [TestFixture]
    public class VsCommandServiceTests : TestBase<VSCommandService>
    {
        private void StubCommandsInterface()
        {
            var dte = GetMock<DTE>();
            var commands = MockRepository.GenerateMock<Commands>();

            commands.Stub(x => x.Item(Arg<object>.Is.Anything, Arg<int>.Is.Anything)).Return(null).WhenCalled(
                arg =>
                {
                    var guid = (string)arg.Arguments[0];
                    var id = (int)arg.Arguments[1];
                    var command = MockRepository.GenerateMock<Command>();
                    command.Stub(cmd => cmd.Name).Return($"{guid}{id}");
                    arg.ReturnValue = command;
                });

            var enumerable = (IEnumerable)commands;
            enumerable.Stub(x => x.GetEnumerator()).Return(Enumerable.Empty<Command>().GetEnumerator());
            dte.Stub(x => x.Commands).Return(commands);
        }

        [Test]
        public void BuildCommandChain_ChainWithNonDeterministicOrder_ExpectNull()
        {
            //Arrange
            var chain = new List<VSCommandService.CommandEventFootprint>
            {
                new VSCommandService.CommandEventFootprint("A", 1, VSCommandService.CommandEventType.BeforeExecute, null),
                new VSCommandService.CommandEventFootprint("B", 2, VSCommandService.CommandEventType.BeforeExecute, null),
                new VSCommandService.CommandEventFootprint("B", 2, VSCommandService.CommandEventType.AfterExecute, null),
                new VSCommandService.CommandEventFootprint("C", 3, VSCommandService.CommandEventType.BeforeExecute, null),
                new VSCommandService.CommandEventFootprint("A", 1, VSCommandService.CommandEventType.AfterExecute, null),
                new VSCommandService.CommandEventFootprint("C", 3, VSCommandService.CommandEventType.AfterExecute, null)
            };

            StubCommandsInterface();

            //Act
            var result = ClassUnderTest.BuildCommandChain(chain);

            //Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void BuildCommandChain_IncompleteChainWithAllMissingBeforeExecute_ExpectTheChainToBeFixed()
        {
            //Arrange
            var chain = new List<VSCommandService.CommandEventFootprint>
            {
                new VSCommandService.CommandEventFootprint("B", 2, VSCommandService.CommandEventType.AfterExecute, null),
                new VSCommandService.CommandEventFootprint("A", 1, VSCommandService.CommandEventType.AfterExecute, null)
            };

            StubCommandsInterface();

            //Act
            var result = ClassUnderTest.BuildCommandChain(chain);

            //Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("A1"));
            Assert.That(result[1].Name, Is.EqualTo("B2"));
        }

        [Test]
        public void BuildCommandChain_IncompleteChainWithOneMissingBeforeExecute_ExpectTheChainToBeFixed()
        {
            //Arrange
            var chain = new List<VSCommandService.CommandEventFootprint>
            {
                new VSCommandService.CommandEventFootprint("B", 2, VSCommandService.CommandEventType.BeforeExecute, null),
                new VSCommandService.CommandEventFootprint("B", 2, VSCommandService.CommandEventType.AfterExecute, null),
                new VSCommandService.CommandEventFootprint("A", 1, VSCommandService.CommandEventType.AfterExecute, null)
            };

            StubCommandsInterface();

            //Act
            var result = ClassUnderTest.BuildCommandChain(chain);

            //Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("A1"));
            Assert.That(result[1].Name, Is.EqualTo("B2"));
        }

        /// <summary>
        ///   In VS there is currently no way only BeforeExecute is triggered, because there is always after execute. Only part
        ///   that can be missing is BeforeExeucte when the command is canceled or replaced.
        /// </summary>
        [Test]
        public void BuildCommandChain_InvalidChain_ExpectNull()
        {
            //Arrange
            var chain = new List<VSCommandService.CommandEventFootprint>
            {
                new VSCommandService.CommandEventFootprint("A", 1, VSCommandService.CommandEventType.BeforeExecute, null),
                new VSCommandService.CommandEventFootprint("B", 2, VSCommandService.CommandEventType.BeforeExecute, null)
            };

            StubCommandsInterface();

            //Act
            var result = ClassUnderTest.BuildCommandChain(chain);

            //Assert
            Assert.That(result, Is.Null);
        }
    }
}