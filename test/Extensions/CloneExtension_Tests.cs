using GPSoftware.Core.Dtos;
using GPSoftware.Core.Extensions;


namespace GPSoftware.core.Tests.Extensions {

    public class CloneExtension_Tests {

        public CloneExtension_Tests() {
        }

        [Fact]
        public void CloneDto() {
            // run
            var dtoSrc1 = new Dto1 {
                PropA = 1,
                PropB = "stringa"
            };
            var dtoDest1 = dtoSrc1.Clone();
            // assert
            dtoDest1.ShouldNotBeSameAs(dtoSrc1);
            dtoDest1.PropA.ShouldBe(dtoSrc1.PropA);
            dtoDest1.PropB.ShouldBe(dtoSrc1.PropB);

            // run
            var dtoSrc2 = new Dto2 {
                PropA = 1,
                PropB = "stringa",
                PropC = dtoSrc1
            };
            var dtoDest2 = dtoSrc2.Clone();
            // assert
            dtoDest2.ShouldNotBeSameAs(dtoSrc2);
            dtoDest2.PropA.ShouldBe(dtoSrc2.PropA);
            dtoDest2.PropB.ShouldBe(dtoSrc2.PropB);
            dtoDest2.PropC.ShouldBe(dtoSrc2.PropC);
        }


        [Fact]
        public void CloneDto_with_readOnly_props() {
            // run
            var dtoSrc = new Dto3WithReadOnly {
                PropA = 1,
                PropB = "thisi is a string",
                PropC = 3,
            };
            var dtoDest = dtoSrc.Clone();
            // assert
            dtoDest.ShouldNotBeSameAs(dtoSrc);
            dtoDest.PropA.ShouldBe(dtoSrc.PropA);
            dtoDest.PropB.ShouldBe(dtoSrc.PropB);
            dtoDest.PropC.ShouldBe(dtoSrc.PropC);
            dtoDest.ReadOnlyProp1.ShouldBe(dtoSrc.ReadOnlyProp1);
            dtoDest.ReadOnlyProp2.ShouldBe(dtoSrc.ReadOnlyProp2);
        }

        [Fact]
        public void CloneDto_deep() {
            var dtoSrc = new Dto2 {
                PropA = 1,
                PropB = "stringa",
                PropC = new Dto1 { PropA = 2, PropB = "inner string" }
            };

            // run
            Dto2 dtoDest = dtoSrc.Clone();

            // assert
            dtoDest.ShouldNotBeSameAs(dtoSrc);
            dtoDest.PropA.ShouldBe(dtoSrc.PropA);
            dtoDest.PropB.ShouldBe(dtoSrc.PropB);
            dtoDest.PropC.ShouldNotBeNull();
            dtoDest.PropC!.ShouldNotBeSameAs(dtoSrc.PropC);
            dtoDest.PropC!.ShouldBe(dtoSrc.PropC);
            dtoDest.PropC!.PropA.ShouldBe(dtoSrc.PropC.PropA);
            dtoDest.PropC!.PropB.ShouldBe(dtoSrc.PropC.PropB);
        }

        [Fact]
        public void CloneDto_into_another_object() {
            var dtoSrc = new Dto2 {
                PropA = 1,
                PropB = "stringa",
                PropC = new Dto1 { PropA = 2, PropB = "inner string" }
            };

            // run
            Dto1 dtoDest = dtoSrc.Clone<Dto2, Dto1>();

            // assert
            dtoDest.ShouldNotBeSameAs(dtoSrc);
            dtoDest.ShouldBeOfType<Dto1>();
            dtoDest.ShouldNotBeOfType<Dto2>();
            dtoDest.PropA.ShouldBe(dtoSrc.PropA);
            dtoDest.PropB.ShouldBe(dtoSrc.PropB);
        }


        // ========================================================================================================

        [Serializable]
        private class Dto1 : IMyDto {
            public int PropA {
                get; set;
            }
            public string? PropB {
                get; set;
            }
        }

        [Serializable]
        private class Dto2 : Dto1, IMyDto {
            public Dto1? PropC {
                get; set;
            }
        }

        [Serializable]
        private class Dto3WithReadOnly : Dto1, IMyDto {
            public int PropC {
                get; set;
            }
            public int ReadOnlyProp1 => 2 * PropA * PropC;
            public string ReadOnlyProp2 => "this is a get only prop copy of " + PropB;
        }
    }
}
