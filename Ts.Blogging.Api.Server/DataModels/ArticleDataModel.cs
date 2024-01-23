using System.ComponentModel.DataAnnotations.Schema;

namespace Ts.Blogging.Api.Server
{
    /// <summary>
    /// The articles database table representational model
    /// </summary>
    public class ArticleDataModel : BaseDataModel
    {
        /// <summary>
        /// The author id foreig key index
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// The title of the article
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The description of this article
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The content of the article
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The image id of this article
        /// </summary>
        public string ImageId { get; set; }

        /// <summary>
        /// The image url of this article
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// The tags of this article
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// The article's author entity relational model
        /// </summary>
        [ForeignKey(nameof(AuthorId))]
        public AuthorDataModel Author { get; set; }
    }
}
