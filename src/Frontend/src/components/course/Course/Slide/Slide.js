import React from "react";

import { Video, CodeMirror, Text, Image, BlocksWrapper, Spoiler, } from "./Blocks";
import CourseLoader from "src/components/course/Course/CourseLoader/CourseLoader";
import blockTypes from "src/components/course/Course/Slide/blockTypes";

import { loadSlide } from "src/actions/course";
import { connect } from "react-redux";
import classNames from 'classnames';
import queryString from "query-string";
import PropTypes from "prop-types";

import styles from './Slide.less';

const mapTypeToBlock = {
	[blockTypes.video]: Video,
	[blockTypes.code]: CodeMirror,
	[blockTypes.text]: Text,
	[blockTypes.tex]: Text,
	[blockTypes.image]: Image,
	[blockTypes.spoiler]: Spoiler,
};

const fullSizeBlockTypes = {
	[blockTypes.video]: true,
	[blockTypes.spoiler]: true,
};

class Slide extends React.Component {
	componentDidMount() {
		const { slideBlocks, } = this.props;
		if(!slideBlocks) {
			this.loadSlide();
		}
	}

	componentDidUpdate(prevProps, prevState, snapshot) {
		if(!this.props.slideBlocks || prevProps.slideId !== this.props.slideId) {
			this.loadSlide();
		}
	}

	loadSlide = () => {
		const { loadSlide, courseId, slideId, } = this.props;
		loadSlide(courseId, slideId);
	}

	render = () => {
		const { slideBlocks, showHiddenBlocks, isHiddenSlide, } = this.props;

		if(!slideBlocks) {
			return (<CourseLoader/>);
		}

		if(showHiddenBlocks) {
			return this.renderSlideBlocks(JSON.parse(JSON.stringify(slideBlocks)), isHiddenSlide);
		}

		if(isHiddenSlide) {
			return this.renderHiddenSlide();
		}

		const slideBlocksForStudent = this.getSlidBlocksForStudents(slideBlocks);


		if(slideBlocksForStudent.length === 0) {
			return this.renderHiddenSlide();
		}

		return this.renderSlideBlocks(JSON.parse(JSON.stringify(slideBlocksForStudent)), isHiddenSlide);
	}

	getSlidBlocksForStudents = (blocks) => {
		const slideBlocksForStudent = [];

		for (const block of blocks) {
			if(block.hide) {
				continue;
			}
			if(block.type === blockTypes.spoiler) {
				this.filterSpoilerBlocksForStudents(block);
			}

			slideBlocksForStudent.push(block);
		}

		return slideBlocksForStudent;
	}

	filterSpoilerBlocksForStudents = (spoiler) => {
		spoiler.blocks = spoiler.blocks.filter(b => !b.hide);
		for (const insideSpoiler of spoiler.blocks.filter(b => b.type === blockTypes.spoiler)) {
			this.filterSpoilerBlocksForStudents(insideSpoiler);
		}
	}

	renderSlideBlocks = (slideBlocks, isHiddenSlide) => {
		this.addAdditionalPropsToBlocks(slideBlocks);
		const blocksPacks = [];

		for (let i = 0; i < slideBlocks.length; i++) {
			const blocksPart = this.getBlocksPack(slideBlocks, i);

			i += blocksPart.blocks.length - 1;
			blocksPacks.push(blocksPart);
		}
		const onlyOneBlock = blocksPacks.length === 1;
		return blocksPacks.map(({ blocks, hide, fullSizeBlocksPack }, i) => {
			return (
				<BlocksWrapper isContainer={ fullSizeBlocksPack }
							   key={ i }
							   showEyeHint={ !fullSizeBlocksPack }
							   isBlock={ !onlyOneBlock }
							   isHidden={ hide }
							   isHeaderOfHiddenSlide={ i === 0 && isHiddenSlide }
				>
					{ blocks.map(this.mapBlockToComponent) }
				</BlocksWrapper>
			)
		});
	}

	renderHiddenSlide = () => {
		return (
			<BlocksWrapper>
				<p>Студенты не увидят этот слайд в навигации</p>
			</BlocksWrapper>
		);
	}

	addAdditionalPropsToBlocks = (slideBlocks) => {
		const { autoplay } = queryString.parse(window.location.search);
		const videoBlocks = slideBlocks.filter(b => b.type === blockTypes.video);

		const firstVideoBlock = videoBlocks[0];
		if(autoplay && firstVideoBlock) {
			firstVideoBlock.autoplay = autoplay ? true : false; //autoplay for first video on slide
		}

		if(firstVideoBlock && slideBlocks.length === 1) {
			firstVideoBlock.openAnnotation = true; // only video on slide => open annotation
		}

		for (const video of videoBlocks) {
			video.isHidden = video.hide;

			const index = slideBlocks.findIndex(b => b === video);

			video.annotationWithoutBottomPaddigns = !video.hide &&
				(index < slideBlocks.length - 1
					? slideBlocks[index + 1].type !== blockTypes.video
					: true);
		}

		for (const texBlock of slideBlocks.filter(b => b.type === blockTypes.tex)) {
			texBlock.content = this.getContentFromTexLines(texBlock);
		}

		for (const spoiler of slideBlocks.filter(b => b.type === blockTypes.spoiler)) {
			const index = slideBlocks.findIndex(b => b === spoiler);
			const { slideId, isHiddenSlide, } = this.props;

			spoiler.blocksId = slideId; // make spoiler close content on slide change
			spoiler.isHidden = spoiler.hide;
			if(index !== 0) {
				spoiler.isPreviousBlockHidden = slideBlocks[index - 1].hide || false;
			} else if(isHiddenSlide) {
				spoiler.isHeaderOfHiddenSlide = true;
			}
			if(spoiler.isHidden) {
				const blocksInHiddenSpoiler = spoiler.blocks.map(b => ({ ...b, hide: true }));
				spoiler.blocks = this.renderSlideBlocks(JSON.parse(JSON.stringify(blocksInHiddenSpoiler)));
			} else {
				spoiler.blocks = this.renderSlideBlocks(JSON.parse(JSON.stringify(spoiler.blocks)));
			}
		}
	}

	getContentFromTexLines = ({ lines }) => {
		return lines.reduce((ac, cv) => ac + `<p class="tex">${ cv }</p>`, '');
	}

	getBlocksPack = (slideBlocks, i) => {
		const block = this.mapElementToBlock(slideBlocks[i]);

		const blocks = [block];
		const blocksPack = { blocks, hide: block.hide, fullSizeBlocksPack: block.fullSizeBlock };

		for (let k = i + 1; k < slideBlocks.length; k++) {
			const otherBlock = this.mapElementToBlock(slideBlocks[k]);
			if(otherBlock.fullSizeBlock === block.fullSizeBlock && otherBlock.hide === block.hide) {
				blocks.push(otherBlock);
			} else break;
		}
		return blocksPack;
	}

	mapElementToBlock = ({ type, hide = false, ...props }) => {
		const typeInLowerCase = type.toLowerCase();

		return {
			Block: mapTypeToBlock[typeInLowerCase],
			fullSizeBlock: fullSizeBlockTypes[typeInLowerCase],
			hide,
			props: { ...props }
		};
	}

	mapBlockToComponent = ({ Block, props }, index, arr) => {
		const className = classNames({ [styles.firstChild]: index === 0 }, { [styles.lastChild]: index === arr.length - 1 });
		return <Block key={ index } className={ className }  { ...props } />;
	}
}


Slide.propTypes = {
	courseId: PropTypes.string.isRequired,
	slideId: PropTypes.string.isRequired,
	slideBlocks: PropTypes.array,
	slideLoading: PropTypes.bool.isRequired,
	loadSlide: PropTypes.func.isRequired,
	showHiddenBlocks: PropTypes.bool,
	isHiddenSlide: PropTypes.bool,
};

Slide.defaultProps = {
	showHiddenBlocks: true,
}

const mapStateToProps = (state, { courseId, slideId, }) => {
	const { slides } = state;
	const { slidesByCourses, slideLoading } = slides;

	const props = {
		courseId,
		slideId,
		slideLoading,
	};

	const coursesSlides = slidesByCourses[courseId];

	if(coursesSlides) {
		props.slideBlocks = coursesSlides[slideId];
	}

	return props;
};

const mapDispatchToProps = (dispatch) => ({
	loadSlide: (courseId, slideId) => dispatch(loadSlide(courseId, slideId)),
});


export default connect(mapStateToProps, mapDispatchToProps)(Slide);
