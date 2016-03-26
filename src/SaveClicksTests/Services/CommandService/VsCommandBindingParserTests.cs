//#region Usings

//using System;
//using System.Windows.Input;
//using NUnit.Framework;
//using SaveClicks.Services.CommandService;
//using SaveClicks.Services.CommandService.Impl;

//#endregion

//namespace SaveClicksTests.Services.CommandService
//{
//    [TestFixture]
//    public class VsCommandBindingParserTests
//    {
//        public class SingleKeyBindingTestData
//        {
//            public SingleKeyBindingTestData(string bindingString, string scope, Key key)
//            {
//                BindingString = bindingString;
//                Scope = scope;
//                Key = key;
//            }

//            public string BindingString { get; }

//            public string Scope { get; }

//            public Key Key { get; }
//        }

//        private static readonly SingleKeyBindingTestData[] SingleKeyBindingTestDataSource =
//        {
//            new SingleKeyBindingTestData("Global::Bkspce", "Global", Key.Back),
//            new SingleKeyBindingTestData("Global::Tab", "Global", Key.Tab),
//            new SingleKeyBindingTestData("Global::Clear", "Global", Key.Clear),
//            new SingleKeyBindingTestData("Global::Enter", "Global", Key.Enter),
//            new SingleKeyBindingTestData("Global::Break", "Global", Key.Pause),
//            new SingleKeyBindingTestData("Global::Esc", "Global", Key.Escape),
//            new SingleKeyBindingTestData("Global::Space", "Global", Key.Space),
//            new SingleKeyBindingTestData("Global::PgUp", "Global", Key.PageUp),
//            new SingleKeyBindingTestData("Global::PgDn", "Global", Key.PageDown),
//            new SingleKeyBindingTestData("Global::End", "Global", Key.End),
//            new SingleKeyBindingTestData("Global::Home", "Global", Key.Home),
//            new SingleKeyBindingTestData("Global::Left Arrow", "Global", Key.Left),
//            new SingleKeyBindingTestData("Global::Up Arrow", "Global", Key.Up),
//            new SingleKeyBindingTestData("Global::Right Arrow", "Global", Key.Right),
//            new SingleKeyBindingTestData("Global::Down Arrow", "Global", Key.Down),
//            new SingleKeyBindingTestData("Global::Ins", "Global", Key.Insert),
//            new SingleKeyBindingTestData("Global::Del", "Global", Key.Delete),
//            new SingleKeyBindingTestData("Global::0", "Global", Key.D0),
//            new SingleKeyBindingTestData("Global::1", "Global", Key.D1),
//            new SingleKeyBindingTestData("Global::2", "Global", Key.D2),
//            new SingleKeyBindingTestData("Global::3", "Global", Key.D3),
//            new SingleKeyBindingTestData("Global::4", "Global", Key.D4),
//            new SingleKeyBindingTestData("Global::5", "Global", Key.D5),
//            new SingleKeyBindingTestData("Global::6", "Global", Key.D6),
//            new SingleKeyBindingTestData("Global::7", "Global", Key.D7),
//            new SingleKeyBindingTestData("Global::8", "Global", Key.D8),
//            new SingleKeyBindingTestData("Global::9", "Global", Key.D9),
//            new SingleKeyBindingTestData("Global::A", "Global", Key.A),
//            new SingleKeyBindingTestData("Global::B", "Global", Key.B),
//            new SingleKeyBindingTestData("Global::C", "Global", Key.C),
//            new SingleKeyBindingTestData("Global::D", "Global", Key.D),
//            new SingleKeyBindingTestData("Global::E", "Global", Key.E),
//            new SingleKeyBindingTestData("Global::F", "Global", Key.F),
//            new SingleKeyBindingTestData("Global::G", "Global", Key.G),
//            new SingleKeyBindingTestData("Global::H", "Global", Key.H),
//            new SingleKeyBindingTestData("Global::I", "Global", Key.I),
//            new SingleKeyBindingTestData("Global::J", "Global", Key.J),
//            new SingleKeyBindingTestData("Global::K", "Global", Key.K),
//            new SingleKeyBindingTestData("Global::L", "Global", Key.L),
//            new SingleKeyBindingTestData("Global::M", "Global", Key.M),
//            new SingleKeyBindingTestData("Global::N", "Global", Key.N),
//            new SingleKeyBindingTestData("Global::O", "Global", Key.O),
//            new SingleKeyBindingTestData("Global::P", "Global", Key.P),
//            new SingleKeyBindingTestData("Global::Q", "Global", Key.Q),
//            new SingleKeyBindingTestData("Global::R", "Global", Key.R),
//            new SingleKeyBindingTestData("Global::S", "Global", Key.S),
//            new SingleKeyBindingTestData("Global::T", "Global", Key.T),
//            new SingleKeyBindingTestData("Global::U", "Global", Key.U),
//            new SingleKeyBindingTestData("Global::V", "Global", Key.V),
//            new SingleKeyBindingTestData("Global::W", "Global", Key.W),
//            new SingleKeyBindingTestData("Global::X", "Global", Key.X),
//            new SingleKeyBindingTestData("Global::Y", "Global", Key.Y),
//            new SingleKeyBindingTestData("Global::Z", "Global", Key.Z),
//            new SingleKeyBindingTestData("Global::Num 0", "Global", Key.NumPad0),
//            new SingleKeyBindingTestData("Global::Num 1", "Global", Key.NumPad1),
//            new SingleKeyBindingTestData("Global::Num 2", "Global", Key.NumPad2),
//            new SingleKeyBindingTestData("Global::Num 3", "Global", Key.NumPad3),
//            new SingleKeyBindingTestData("Global::Num 4", "Global", Key.NumPad4),
//            new SingleKeyBindingTestData("Global::Num 5", "Global", Key.NumPad5),
//            new SingleKeyBindingTestData("Global::Num 6", "Global", Key.NumPad6),
//            new SingleKeyBindingTestData("Global::Num 7", "Global", Key.NumPad7),
//            new SingleKeyBindingTestData("Global::Num 8", "Global", Key.NumPad8),
//            new SingleKeyBindingTestData("Global::Num 9", "Global", Key.NumPad9),
//            new SingleKeyBindingTestData("Global::Num *", "Global", Key.Multiply),
//            new SingleKeyBindingTestData("Global::Num +", "Global", Key.Add),
//            new SingleKeyBindingTestData("Global::Num -", "Global", Key.Subtract),
//            new SingleKeyBindingTestData("Global::Num .", "Global", Key.Decimal),
//            new SingleKeyBindingTestData("Global::Num /", "Global", Key.Divide),
//            new SingleKeyBindingTestData("Global::F1", "Global", Key.F1),
//            new SingleKeyBindingTestData("Global::F2", "Global", Key.F2),
//            new SingleKeyBindingTestData("Global::F3", "Global", Key.F3),
//            new SingleKeyBindingTestData("Global::F4", "Global", Key.F4),
//            new SingleKeyBindingTestData("Global::F5", "Global", Key.F5),
//            new SingleKeyBindingTestData("Global::F6", "Global", Key.F6),
//            new SingleKeyBindingTestData("Global::F7", "Global", Key.F7),
//            new SingleKeyBindingTestData("Global::F8", "Global", Key.F8),
//            new SingleKeyBindingTestData("Global::F9", "Global", Key.F9),
//            new SingleKeyBindingTestData("Global::F10", "Global", Key.F10),
//            new SingleKeyBindingTestData("Global::F11", "Global", Key.F11),
//            new SingleKeyBindingTestData("Global::F12", "Global", Key.F12),
//            new SingleKeyBindingTestData("Global::F13", "Global", Key.F13),
//            new SingleKeyBindingTestData("Global::F14", "Global", Key.F14),
//            new SingleKeyBindingTestData("Global::F15", "Global", Key.F15),
//            new SingleKeyBindingTestData("Global::F16", "Global", Key.F16),
//            new SingleKeyBindingTestData("Global::F17", "Global", Key.F17),
//            new SingleKeyBindingTestData("Global::F18", "Global", Key.F18),
//            new SingleKeyBindingTestData("Global::F19", "Global", Key.F19),
//            new SingleKeyBindingTestData("Global::F20", "Global", Key.F20),
//            new SingleKeyBindingTestData("Global::F21", "Global", Key.F21),
//            new SingleKeyBindingTestData("Global::F22", "Global", Key.F22),
//            new SingleKeyBindingTestData("Global::F23", "Global", Key.F23),
//            new SingleKeyBindingTestData("Global::F24", "Global", Key.F24)
//        };

//        [Test]
//        [TestCase(null)]
//        [TestCase("")]
//        [TestCase("  ")]
//        public void Parse_EmptyNullOrWhitespaceInput_ExpectArgumentExceptionThrown(string input)
//        {
//            //Arrange
//            var parser = new VsCommandBindingParser();

//            //Act
//            //Assert
//            Assert.That(() => parser.Parse(input), Throws.ArgumentException,
//                "The binding cannot be null, empty or whitespace. Valid binding string must be provided.");
//        }

//        [Test]
//        [TestCase("GlobalF1")]
//        [TestCase("JustRandomString")]
//        public void Parse_InputWithoutScopeSeparator_ExpectArgumentExceptionThrown(string input)
//        {
//            //Arrange
//            var parser = new VsCommandBindingParser();

//            //Act
//            //Assert
//            Assert.That(() => parser.Parse(input), Throws.Exception.TypeOf<FormatException>(),
//                $"The binding string '{input}' is not in correct format. Scope separator '::' was not found thus scope of the command couldn't be determined.");
//        }

//        [Test]
//        [TestCaseSource(nameof(SingleKeyBindingTestDataSource))]
//        public void Parse_SingleKeyCommandBinding_ExpectSingleKeyCommandBindingClassWithCorrectValues(
//            SingleKeyBindingTestData testData)
//        {
//            //Arrange
//            var parser = new VsCommandBindingParser();

//            //Act
//            var result = parser.Parse(testData.BindingString);

//            //Assert
//            Assert.That(result, Is.TypeOf<SingleKeyCommandBinding>());
//            Assert.That(result.Scope, Is.EqualTo(testData.Scope));
//            Assert.That(((SingleKeyCommandBinding) result).Key, Is.EqualTo(testData.Key));
//        }
//    }
//}