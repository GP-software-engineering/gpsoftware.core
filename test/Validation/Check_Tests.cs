namespace GPSoftware.Core.Validation {

    public class Check_Tests {

        #region NotNull

        [Fact]
        public void NotNull_Should_Return_Object_If_Not_Null() {
            var obj = new object();
            var result = Check.NotNull(obj, "obj");
            result.ShouldBe(obj);
        }

        [Fact]
        public void NotNull_Should_Throw_ArgumentNullException_If_Null() {
            object? obj = null;
            var ex = Assert.Throws<ArgumentNullException>(() => Check.NotNull(obj, "obj"));
            ex.ParamName.ShouldBe("obj");
        }

        #endregion

        #region String Lengths (NotNull, NotNullOrWhiteSpace, NotNullOrEmpty)

        [Theory]
        [InlineData("valid", 10, 0)]
        [InlineData("exact", 5, 5)]
        public void StringChecks_Should_Return_String_If_Valid(string input, int max, int min) {
            Check.NotNull(input, "param", max, min).ShouldBe(input);
            Check.NotNullOrEmpty(input, "param", max, min).ShouldBe(input);
            Check.NotNullOrWhiteSpace(input, "param", max, min).ShouldBe(input);
        }

        [Fact]
        public void NotNullOrWhiteSpace_Should_Throw_If_Only_Spaces() {
            Assert.Throws<ArgumentException>(() => Check.NotNullOrWhiteSpace("   ", "param"));
        }

        [Fact]
        public void NotNullOrEmpty_Should_Throw_If_Empty() {
            Assert.Throws<ArgumentException>(() => Check.NotNullOrEmpty("", "param"));
        }

        [Fact]
        public void StringChecks_Should_Throw_If_Exceeds_MaxLength() {
            Assert.Throws<ArgumentException>(() => Check.NotNull("toolong", "param", maxLength: 5));
        }

        [Fact]
        public void StringChecks_Should_Throw_If_Below_MinLength() {
            Assert.Throws<ArgumentException>(() => Check.NotNull("tiny", "param", minLength: 10));
        }

        #endregion

        #region NotNullOrDefault

        [Fact]
        public void NotNullOrDefault_Should_Return_Value_If_Valid() {
            int? validInt = 5;
            Check.NotNullOrDefault(validInt, "validInt").ShouldBe(5);
        }

        [Fact]
        public void NotNullOrDefault_Should_Throw_If_Null() {
            int? nullInt = null;
            Assert.Throws<ArgumentNullException>(() => Check.NotNullOrDefault(nullInt, "nullInt"));
        }

        [Fact]
        public void NotNullOrDefault_Should_Throw_If_Default_Value() {
            int? defaultInt = 0; // 0 is default for int
            Assert.Throws<ArgumentException>(() => Check.NotNullOrDefault(defaultInt, "defaultInt"));
        }

        #endregion

        #region NotDefault (Guid)

        [Fact]
        public void NotDefault_Should_Return_Guid_If_Valid() {
            var guid = Guid.NewGuid();
            Check.NotDefault(guid, "guid").ShouldBe(guid);
        }

        [Fact]
        public void NotDefault_Should_Throw_If_Empty_Guid() {
            Assert.Throws<ArgumentException>(() => Check.NotDefault(Guid.Empty, "guid"));
        }

        #endregion

        #region NotNullOrEmpty (ICollection)

        [Fact]
        public void Collection_NotNullOrEmpty_Should_Return_If_Valid() {
            var list = new List<int> { 1, 2, 3 };
            var result = Check.NotNullOrEmpty(list, "list");
            result.Count.ShouldBe(3);
        }

        [Fact]
        public void Collection_NotNullOrEmpty_Should_Throw_If_Null() {
            List<int>? list = null;
            Assert.Throws<ArgumentException>(() => Check.NotNullOrEmpty(list, "list"));
        }

        [Fact]
        public void Collection_NotNullOrEmpty_Should_Throw_If_Empty() {
            var list = new List<int>();
            Assert.Throws<ArgumentException>(() => Check.NotNullOrEmpty(list, "list"));
        }

        #endregion

        #region Range

        [Theory]
        [InlineData(5, 1, 10)]
        [InlineData(0, 0, 100)]
        [InlineData(-50, -100, 0)]
        public void Range_Should_Return_Value_If_Within_Bounds(int val, int min, int max) {
            Check.Range(val, min, max, "val").ShouldBe(val);
        }

        [Fact]
        public void Range_Should_Throw_If_Out_Of_Bounds() {
            Assert.Throws<ArgumentOutOfRangeException>(() => Check.Range(15, 0, 10, "val"));
            Assert.Throws<ArgumentOutOfRangeException>(() => Check.Range(-5, 0, 10, "val"));
        }

        [Fact]
        public void Range_Decimal_Should_Work_Correctly() {
            Check.Range(0m, 0m, 100m, "dec").ShouldBe(0m);
            Assert.Throws<ArgumentOutOfRangeException>(() => Check.Range(100.1m, 0m, 100m, "dec"));
        }

        #endregion

        #region Length (IEnumerable)

        [Fact]
        public void Length_Should_Return_Sequence_If_Within_Bounds() {
            IEnumerable<int> sequence = new[] { 1, 2, 3, 4, 5 };
            var result = Check.Length(sequence, 1, 5, "sequence");
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public void Length_Should_Throw_If_Exceeds_MaxLength() {
            IEnumerable<int> sequence = new[] { 1, 2, 3, 4, 5 };
            Assert.Throws<ArgumentException>(() => Check.Length(sequence, 1, 3, "sequence"));
        }

        [Fact]
        public void Length_Should_Throw_If_Below_MinLength() {
            IEnumerable<int> sequence = new[] { 1, 2 };
            Assert.Throws<ArgumentException>(() => Check.Length(sequence, 5, 10, "sequence"));
        }

        #endregion
    }
}
