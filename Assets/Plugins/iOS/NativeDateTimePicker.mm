#import <UIKit/UIKit.h>

typedef void (*DateTimePickerCallback)(int year, int month, int day,
                                       int hour, int minute, bool cancelled);

static DateTimePickerCallback _dateTimePickerCallback = NULL;

extern "C" {

    UIViewController* GetRootViewController() {
        UIWindow *window = nil;
        for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
            if (scene.activationState == UISceneActivationStateForegroundActive &&
                [scene isKindOfClass:[UIWindowScene class]]) {
                UIWindowScene *windowScene = (UIWindowScene *)scene;
                for (UIWindow *w in windowScene.windows) {
                    if (w.isKeyWindow) {
                        window = w;
                        break;
                    }
                }
            }
        }
        return window.rootViewController;
    }

    void _ShowNativeDateTimePicker(int mode, int year, int month, int day,
                                    int hour, int minute,
                                    bool hasMin, long minUnixMs,
                                    bool hasMax, long maxUnixMs,
                                    DateTimePickerCallback callback)
    {
        _dateTimePickerCallback = callback;

        dispatch_async(dispatch_get_main_queue(), ^{
            UIViewController *rootVC = GetRootViewController();
            if (!rootVC) {
                if (_dateTimePickerCallback)
                    _dateTimePickerCallback(0, 0, 0, 0, 0, true);
                return;
            }

            UIDatePicker *picker = [[UIDatePicker alloc] init];
            picker.translatesAutoresizingMaskIntoConstraints = NO;

            // Set picker mode
            if (mode == 0)
                picker.datePickerMode = UIDatePickerModeDate;
            else if (mode == 1)
                picker.datePickerMode = UIDatePickerModeTime;
            else
                picker.datePickerMode = UIDatePickerModeDateAndTime;

            // Use wheels style for better UX
            if (@available(iOS 13.4, *))
                picker.preferredDatePickerStyle = UIDatePickerStyleWheels;

            // Set initial date
            NSCalendar *cal = [NSCalendar currentCalendar];
            NSDateComponents *comps = [[NSDateComponents alloc] init];
            comps.year = year;
            comps.month = month;
            comps.day = day;
            comps.hour = hour;
            comps.minute = minute;
            picker.date = [cal dateFromComponents:comps];

            // Set min/max dates
            if (hasMin) {
                picker.minimumDate = [NSDate dateWithTimeIntervalSince1970:minUnixMs / 1000.0];
            }
            if (hasMax) {
                picker.maximumDate = [NSDate dateWithTimeIntervalSince1970:maxUnixMs / 1000.0];
            }

            // Create alert controller with picker
            UIAlertController *alert = [UIAlertController
                alertControllerWithTitle:@"\n\n\n\n\n\n\n\n\n\n\n"
                message:nil
                preferredStyle:UIAlertControllerStyleActionSheet];

            [alert.view addSubview:picker];

            // Layout constraints for picker
            [NSLayoutConstraint activateConstraints:@[
                [picker.leadingAnchor constraintEqualToAnchor:alert.view.leadingAnchor],
                [picker.trailingAnchor constraintEqualToAnchor:alert.view.trailingAnchor],
                [picker.topAnchor constraintEqualToAnchor:alert.view.topAnchor constant:8],
                [picker.heightAnchor constraintEqualToConstant:216]
            ]];

            // Done action
            [alert addAction:[UIAlertAction actionWithTitle:@"Done"
                style:UIAlertActionStyleDefault
                handler:^(UIAlertAction *action) {
                    NSDateComponents *result = [cal components:
                        NSCalendarUnitYear | NSCalendarUnitMonth | NSCalendarUnitDay |
                        NSCalendarUnitHour | NSCalendarUnitMinute
                        fromDate:picker.date];
                    if (_dateTimePickerCallback)
                        _dateTimePickerCallback(
                            (int)result.year, (int)result.month, (int)result.day,
                            (int)result.hour, (int)result.minute, false);
                }]];

            // Cancel action
            [alert addAction:[UIAlertAction actionWithTitle:@"Cancel"
                style:UIAlertActionStyleCancel
                handler:^(UIAlertAction *action) {
                    if (_dateTimePickerCallback)
                        _dateTimePickerCallback(0, 0, 0, 0, 0, true);
                }]];

            // iPad support - popover presentation
            if (alert.popoverPresentationController) {
                alert.popoverPresentationController.sourceView = rootVC.view;
                alert.popoverPresentationController.sourceRect = CGRectMake(
                    CGRectGetMidX(rootVC.view.bounds),
                    CGRectGetMidY(rootVC.view.bounds),
                    0, 0);
                alert.popoverPresentationController.permittedArrowDirections = 0;
            }

            [rootVC presentViewController:alert animated:YES completion:nil];
        });
    }
}
