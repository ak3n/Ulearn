import React, {Component} from 'react';
import {Switch, Route, BrowserRouter, Redirect} from 'react-router-dom';

import AnyPage from "./pages/AnyPage";
import ErrorBoundary from "./components/common/ErrorBoundary";
import NotFoundErrorBoundary from "./components/common/Error/NotFoundErrorBoundary";
import YandexMetrika from "./components/common/YandexMetrika";
import Header from "./components/common/Header";
import {Provider, connect} from "react-redux";
import thunkMiddleware from "redux-thunk"
import {createLogger} from "redux-logger"
import {applyMiddleware, createStore} from "redux";

import rootReducer from "./redux/reducers"
import api from "./api"
import Toast from "@skbkontur/react-ui/components/Toast/Toast";
import GroupListPage from "./pages/course/groups/GroupListPage";
import GroupPage from "./pages/course/groups/GroupPage";
import Course from './pages/course/CoursePage';
import UnitPage from "./components/cards/UnitPage/UnitPage";
import CoursePage from "./components/cards/CoursePage/CoursePage";
import {getQueryStringParameter} from "./utils";

import styles from "./App.less"
import shortQuestionsExample from "./components/cards/UnitPage/ShortQuestions/shortQuestionsExample";

let loggerMiddleware = createLogger();

function configureStore(preloadedState) {
	let env = process.env.NODE_ENV || 'development';
	let isDevelopment = env === 'development';

	let middlewares = isDevelopment ?
		applyMiddleware(thunkMiddleware, loggerMiddleware) :
		applyMiddleware(thunkMiddleware);

	return createStore(
		rootReducer,
		preloadedState,
		middlewares
	)
}

let store = configureStore({
	account: {
		isAuthenticated: false,
		isSystemAdministrator: false,
		roleByCourse: {},
		accessesByCourse: {},
	},
	notifications: {
		count: 0,
		lastTimestamp: undefined
	}
});
//TODO(rozentor)  remove this 2 const
const guides = [
	"Подумайте над вопросом, перед тем как смотреть ответ.",
	"Оцените, на сколько хорошо вы знали ответ. Карточки, которые вы знаете плохо, будут показываться чаще",
	"Регулярно пересматривайте карточки, даже если вы уверенны в своих знаниях. Чем чаще использовать карточки, тем лучше они запоминаются."
];

const unitPageProps = {
	unitTitle: 'UnitTitle',
	statistics: {notRated: 3, rate1: 0, rate2: 0, rate3: 0, rate4: 0, rate5: 2},
	totalFlashcardsCount: 5,
	guides: guides,
	questionsWithAnswers: shortQuestionsExample
};

// Update notifications count each minute
setInterval(() => {
	if (store.getState().account.isAuthenticated)
		store.dispatch(api.notifications.getNotificationsCount(store.getState().notifications.lastTimestamp))
}, 60 * 1000);

api.setServerErrorHandler((message) => Toast.push(message ? message : 'Произошла ошибка. Попробуйте перезагрузить страницу.'));

class UlearnApp extends Component {
	render() {
		let pathname = window.location.pathname.toLowerCase();
		let isLti = pathname.endsWith('/ltislide') || pathname.endsWith('/acceptedalert');
		let isHeaderVisible = !isLti;

		return (
			<Provider store={store}>
				<InternalUlearnApp isHeaderVisible={isHeaderVisible}/>
			</Provider>
		)
	}
}

class InternalUlearnApp extends Component {
	constructor(props) {
		super(props);
		this.state = {
			initializing: true,
		}
	}

	componentDidMount() {
		this.props.getCurrentUser();
		this.props.getCourses();
	}

	componentWillReceiveProps(nextProps, nextState) {
		this.setState({
			initializing: false
		});
		if (!this.props.account.isAuthenticated && nextProps.account.isAuthenticated) {
			this.props.getNotificationsCount();
		}
	}

	render() {
		const isHeaderVisible = this.props.isHeaderVisible;
		return (
			<BrowserRouter>
				<ErrorBoundary>
					{isHeaderVisible &&
					<React.Fragment>
						<Header initializing={this.state.initializing}/>
						<div className={styles.headerContentDivider} id='headerContentDivider'/>
					</React.Fragment>
					}
					<NotFoundErrorBoundary>
						{!this.state.initializing && // Avoiding bug: don't show page while initializing.
						// Otherwise we make two GET requests sequentially.
						// Unfortunately some our GET handlers are not idempotent (i.e. /Admin/CheckNextExerciseForSlide)
						<Switch>
							<Route path="/Admin/Groups" component={redirectLegacyPage("/:courseId/groups")}/>

							<Route path="/course/:courseId/" component={Course} exact/>
							<Route path="/course/:courseId/:slideId" component={Course} exact/>

							<Route path="/:courseId/groups/" component={GroupListPage} exact/>
							<Route path="/:courseId/groups/:groupId/" component={GroupPage} exact/>
							<Route path="/:courseId/groups/:groupId/:groupPage" component={GroupPage} exact/>

							<Route exact path="/:courseId/flashcards/"
								   render={(props) => <CoursePage {...props} guides={guides}/>}/>
							<Route exact path="/:courseId/flashcards/:unitId/"
								   render={(props) => <UnitPage {...props} {...unitPageProps}/>}/>

							<Route component={AnyPage}/>
						</Switch>
						}
					</NotFoundErrorBoundary>
					<YandexMetrika/>
				</ErrorBoundary>
			</BrowserRouter>
		);
	}

	static mapStateToProps(state) {
		return {
			account: state.account,
		}
	}

	static mapDispatchToProps(dispatch) {
		return {
			getCurrentUser: () => dispatch(api.account.getCurrentUser()),
			getCourses: () => dispatch(api.courses.getCourses()),
			getNotificationsCount: () => dispatch(api.notifications.getNotificationsCount())
		}
	}
}

InternalUlearnApp = connect(InternalUlearnApp.mapStateToProps, InternalUlearnApp.mapDispatchToProps)(InternalUlearnApp);

function redirectLegacyPage(to) {
	return class extends Component {
		constructor(props) {
			super(props);
			let courseId = getQueryStringParameter("courseId");
			if (courseId)
				to = to.replace(":courseId", courseId);
		}

		render() {
			return <Redirect to={to}/>;
		}
	};
}

export default UlearnApp;
