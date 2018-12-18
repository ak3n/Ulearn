import React, { Component } from "react";
import PropTypes from "prop-types";
import api from "../../../../../api/index";
import ComboBox from "@skbkontur/react-ui/components/ComboBox/ComboBox";
import Avatar from "../Avatar/Avatar";

import styles from './style.less';

class ComboboxInstructorsSearch extends Component {

	render () {
		const { selected } = this.props;
		return (
		<ComboBox
			getItems={this.getItems}
			size="small"
			width="100%"
			value={selected}
			renderItem={this.renderItem}
			renderValue={this.renderItem}
			renderNotFound={this.renderNotFound}
			onChange={this.onChangeItem}
			placeholder="Начните вводить имя, фамилию или логин преподавателя"/>
		);
	}

	getItems = (query) => {
		const { accesses, owner } = this.props;
		const includes = (str, substr) => str.toLowerCase().includes(substr.toLowerCase());
		const isAddedUser = (item) => {
			return (owner.id !== item.id) &&
			(accesses.filter(i => i.user.id === item.id)).length === 0;
		};

		return api.users.getCourseInstructors(this.props.courseId)
			.then(json => {
				return json.instructors
					.filter(item => {
						return (isAddedUser(item)) &&
							(includes(item.visible_name, query) ||
							includes(item.login, query))
					})
					.map(item => ({
								value: item.id,
								label: item.visible_name,
								...item,
							}
						)
					)
				})
			.catch(error => {
				console.error(error);
				return [];
			});
	};

	renderItem = (item) => {
		const name = item.label;

		return (
		<div className={styles["teacher"]}>
			<Avatar user={item} size='small'/>
			<span>{name}</span>
			<span className={styles["teacher_login"]}>логин: {item.login}</span>
		</div>
		)
	};

	renderNotFound = () => {
			return (
				<span>
					В этом курсе нет свободных преподавателей
				</span>
			)
	};

	onChangeItem = (_, item) => {
		this.props.onAddTeacher(item);
	}
}

ComboboxInstructorsSearch.propTypes = {
	selected: PropTypes.object,
	courseId: PropTypes.string,
	onAddTeacher: PropTypes.func,
	accesses: PropTypes.array,
	owner: PropTypes.object,
};

export default ComboboxInstructorsSearch;