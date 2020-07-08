// <copyright file="TagsValidationAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Helpers.CustomValidations
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;

    /// <summary>
    /// Validate tag based on length and tag count for post.
    /// </summary>
    public sealed class TagsValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagsValidationAttribute"/> class.
        /// </summary>
        /// <param name="maxCount">Max count of tags for validation.</param>
        /// <param name="maxTagLength">Max supported character length of tags.</param>
        public TagsValidationAttribute(int maxCount, int maxTagLength = 20)
        {
            this.MaxCount = maxCount;
            this.MaxTagLength = maxTagLength;
        }

        /// <summary>
        /// Gets max count of tags for validation.
        /// </summary>
        public int MaxCount { get; }

        /// <summary>
        /// Gets max tag length for validation.
        /// </summary>
        public int MaxTagLength { get; }

        /// <summary>
        /// Validate tag based on tag length and number of tags separated by comma.
        /// </summary>
        /// <param name="value">String containing tags separated by comma.</param>
        /// <param name="validationContext">Context for getting object which needs to be validated.</param>
        /// <returns>Validation result (either error message for failed validation or success).</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var tags = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(tags))
            {
                var tagsList = tags.Split(';');

                if (tagsList.Length > this.MaxCount)
                {
                    return new ValidationResult("Max tags count exceeded");
                }

                foreach (var tag in tagsList)
                {
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        return new ValidationResult("Tag cannot be null or empty");
                    }

                    if (tag.Length > this.MaxTagLength)
                    {
                        return new ValidationResult("Max tag length exceeded");
                    }
                }
            }

            // Tags are not mandatory for adding/updating post
            return ValidationResult.Success;
        }
    }
}
