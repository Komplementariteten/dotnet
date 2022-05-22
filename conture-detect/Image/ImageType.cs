namespace Image {
    public enum ImageType {
        PNG,
        UNKOWN,
        BMP
    }

    [Flags]
    public enum MarkType {
        MarkConture,
        MarkCluster,
    }
}