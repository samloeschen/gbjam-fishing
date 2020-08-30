using System;
public interface Range<T> where T: struct, IComparable, IComparable<T>, IEquatable<T> { 
    T min { get; set; }
    T  max {get; set; }
    bool Contains(T value);
}