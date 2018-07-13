import { combineReducers } from "redux"

const initialAccountState = {
    isAuthenticated: false,
    isSystemAdministrator: false,
    roleByCourse: {}
};

function account(state = initialAccountState, action) {
    switch (action.type) {
        case 'ACCOUNT__USER_INFO_UPDATED':
            let newState = {...state};
            newState.isAuthenticated = action.isAuthenticated;
            if (newState.isAuthenticated) {
                newState.login = action.login;
                newState.firstName = action.firstName;
                newState.lastName = action.lastName;
            }
            return newState;
        case 'ACCOUNT__USER_ROLES_UPDATED':
            return {
                ...state,
                isSystemAdministrator: action.isSystemAdministrator,
                roleByCourse: action.roleByCourse
            };
        default:
            return state;
    }
}

const initialCoursesState = {
    courseById: {}
};

function courses(state = initialCoursesState, action) {
    switch (action.type) {
        case 'COURSES__UPDATED':
            return {
                ...state,
                courseById: action.courseById
            };
        default:
            return state;
    }
}

const initialNotificationsState = {
    count: 0,
    lastTimestamp: ""
};

function notifications(state = initialNotificationsState, action) {
    switch (action.type) {
        case 'NOTIFICATIONS__COUNT_UPDATED':
            return {
                ...state,
                count: action.count,
                lastTimestamp: action.lastTimestamp
            };
        default:
            return state;
    }
}

const rootReducer = combineReducers({
    account,
    courses,
    notifications
});

export default rootReducer;