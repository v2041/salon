'use client'
import * as React from 'react';
import Box from '@mui/material/Box';
import BottomNavigation from '@mui/material/BottomNavigation';
import BottomNavigationAction from '@mui/material/BottomNavigationAction';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import ContentCutIcon from '@mui/icons-material/ContentCut';
import {Paper} from "@mui/material";

export default function BottomNavigationBar() {
    const [value, setValue] = React.useState(0);
    return (
        <Paper
            elevation={3}
            sx={{
                position: 'fixed',
                display: 'flex',
                bottom: 0,
                left: 0,
                right: 0,
                width: '100%',
                justifyContent: 'center',
                backgroundColor: 'transparent',
                boxShadow: 'none'
            }}>
            <BottomNavigation
                showLabels
                value={value}
                onChange={(event, newValue) => {
                    setValue(newValue);
                }}
                sx={{
                    width: {
                        xs: '100%',
                        sm: '400px'
                    },
                    '& .MuiBottomNavigationAction-root': {
                        minWidth: 0,
                        maxWidth: 'none',
                        flex: 1
                    },
                    boxShadow: 3,
                    borderRadius: {
                        xs: '0px',
                        sm: '16px 16px 0 0'
                    },
                    backgroundColor: 'background.paper'
                }}
            >
                <BottomNavigationAction label="Профиль" icon={<AccountCircleIcon/>}/>
                <BottomNavigationAction label="Услуги" icon={<ContentCutIcon/>}/>
                <BottomNavigationAction label="Записи" icon={<AutoStoriesIcon/>}/>
            </BottomNavigation>
        </Paper>
    );
}