export const coursePath = "course";
export const flashcards = "flashcards";
export const commentsPath = "comments";
export const commentPoliciesPath = "comment-policies";
export const courseStatistics = "/analytics/courseStatistics";
export const slides = "slides";
export const ltiSlide = "ltislide";
export const acceptedAlert = "acceptedalert";
export const acceptedSolutions = "acceptedsolutions";
export const resetStudentsLimits = "/students/reset-limits";
export const signalrWS = 'ws';

export function constructPathToSlide(courseId, slideId) {
	return `/${ coursePath }/${ courseId }/${ slideId }`;
}

export function constructPathToComment(commentId, isLike) {
	const url = `${ commentsPath }/${ commentId }`;

	if (isLike) {
		return url + "/like";
	}

	return url;
}
